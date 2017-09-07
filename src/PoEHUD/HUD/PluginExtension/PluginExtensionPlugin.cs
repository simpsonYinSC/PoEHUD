using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PoEHUD.Controllers;
using PoEHUD.HUD.Interfaces;
using PoEHUD.HUD.Menu;
using PoEHUD.Models;
using PoEHUD.Plugins;
using Trinet.Core.IO.Ntfs;
using Graphics = PoEHUD.HUD.UI.Graphics;

namespace PoEHUD.HUD.PluginExtension
{
    public class PluginExtensionPlugin : IPlugin
    {
        public const string UpdateTempDir = "%PluginUpdate%"; // Do not change this value. Otherwice this value in PoeHUD_PluginsUpdater plugin should be also changed.
        public const string UpdateBackupDir = "%Backup%";
        public static List<BasePlugin> Plugins = new List<BasePlugin>();
        public readonly GameController GameController;
        public readonly Graphics Graphics;
        private const string ZoneName = "Zone.Identifier";
        private List<string> pluginUpdateLog;

        public PluginExtensionPlugin(GameController gameController, Graphics graphics)
        {
            GameController = gameController;
            Graphics = graphics;
            SearchPlugins();
            LoadSettings();  
            InitMenuForPlugins(); 
            InitPlugins();
            gameController.EntityListWrapper.EntityAdded += OnEntityAdded;
            gameController.EntityListWrapper.EntityRemoved += OnEntityRemoved;
        }

        public event Action ExternalInitialise = delegate { };
        public event Action ExternalOnRender = delegate { };
        public event Action<EntityWrapper> ExternalEntityAdded = delegate { };
        public event Action<EntityWrapper> ExternalEntityRemoved = delegate { };
        public event Action<MenuItem> ExternalInitMenu = delegate { };
        public event Action ExternalLoadSettings = delegate { };
        public event Action ExternalClose = delegate { };

        public static void LogError(object message, float displayTime)
        {
            DebugPlugin.DebugPlugin.LogMessage(message, displayTime, SharpDX.Color.Red);
        }

        public static void LogMessage(object message, float displayTime)
        {
            DebugPlugin.DebugPlugin.LogMessage(message, displayTime);
        }

        private static bool ProcessFile_Real(string path)
        {
            bool result = FileSystem.AlternateDataStreamExists(path, ZoneName);
            if (!result)
            {
                return false;
            }

            // Clear the read-only attribute, if set:
            FileAttributes attributes = File.GetAttributes(path);
            if (FileAttributes.ReadOnly == (FileAttributes.ReadOnly & attributes))
            {
                attributes &= ~FileAttributes.ReadOnly;
                File.SetAttributes(path, attributes);
            }

            FileSystem.DeleteAlternateDataStream(path, ZoneName);
            result = FileSystem.AlternateDataStreamExists(path, ZoneName); // Check again

            return result;
        }

        private void InitMenuForPlugins()
        {
            RootButton mainMenu = MenuPlugin.MenuRootButton;
            var pluginsMenu = MenuPlugin.AddChild(mainMenu, "Plugins", true);
            ExternalInitMenu(pluginsMenu);
        }

        private void SearchPlugins()
        {
            DirectoryInfo pluginsDir = new DirectoryInfo("plugins");
            if (!pluginsDir.Exists)
            {
                return;
            }

            foreach (var pluginDirectoryInfo in pluginsDir.GetDirectories())
            {
                string pluginTempUpdateDir = Path.Combine(pluginDirectoryInfo.FullName, UpdateTempDir);

                if (Directory.Exists(pluginTempUpdateDir))
                {
                    pluginUpdateLog = new List<string>();

                    string backupDir = Path.Combine(pluginDirectoryInfo.FullName, UpdateBackupDir);

                    if (Directory.Exists(backupDir))
                    {
                        FileOperationApiWrapper.MoveToRecycleBin(backupDir);
                    }

                    string logFilePAth = Path.Combine(pluginDirectoryInfo.FullName, "%PluginUpdateLog.txt");
                    if (File.Exists(logFilePAth))
                    {
                        File.Delete(logFilePAth);
                    }

                    if (MoveDirectoryFiles(pluginDirectoryInfo.FullName, pluginTempUpdateDir, pluginDirectoryInfo.FullName))
                    {
                        pluginUpdateLog.Add("Deleting temp dir:\t" + pluginTempUpdateDir);
                        Directory.Delete(pluginTempUpdateDir, true);
                    }
                    else
                    {
                        LogMessage("PoeHUD PluginUpdater: some files wasn't moved or replaced while update (check %PluginUpdateLog.txt). You can move them manually: " + pluginTempUpdateDir, 20);
                        File.WriteAllLines(logFilePAth, pluginUpdateLog.ToArray());
                    }
                }

                FileInfo[] directoryDlls = pluginDirectoryInfo.GetFiles("*.dll", SearchOption.TopDirectoryOnly);

                foreach (var dll in directoryDlls)
                {
                    TryLoadDll(dll.FullName, pluginDirectoryInfo.FullName);
                }
            }
        }

        private bool MoveDirectoryFiles(string origDirectory, string sourceDirectory, string targetDirectory)
        {
            bool noErrors = true;
            var sourceDirectoryInfo = new DirectoryInfo(sourceDirectory);

            foreach (var file in sourceDirectoryInfo.GetFiles())
            {
                string destFile = Path.Combine(targetDirectory, file.Name);
                bool fileExist = File.Exists(destFile);
                
                try
                {
                    string fileLocalPath = destFile.Replace(origDirectory, string.Empty);

                    if (fileExist)
                    {
                        var backupPath = origDirectory + @"\" + UpdateBackupDir + fileLocalPath; // Do not use Path.Combine due to Path.IsPathRooted checks
                        var backupDirPath = Path.GetDirectoryName(backupPath);

                        if (!Directory.Exists(backupDirPath))
                        {
                            if (backupDirPath != null)
                            {
                                Directory.CreateDirectory(backupDirPath);
                            }
                        }

                        File.Copy(destFile, backupPath, true);
                    }

                    File.Copy(file.FullName, destFile, true);
                    File.Delete(file.FullName); // Delete from temp update dir

                    if (fileExist)
                    {
                        pluginUpdateLog.Add("File Replaced:\t\t" + destFile + " vs " + file.FullName);
                    }
                    else
                    {
                        pluginUpdateLog.Add("File Added:\t\t\t" + destFile);
                    }
                }
                catch (Exception ex)
                {
                    noErrors = false;
                    if (fileExist)
                    {
                        LogError("PoeHUD PluginUpdater: can't replace file: " + destFile + ", Error: " + ex.Message, 10);
                        pluginUpdateLog.Add("Error replacing file: \t" + destFile);
                    }
                    else
                    {
                        LogError("PoeHUD PluginUpdater: can't move file: " + destFile + ", Error: " + ex.Message, 10);
                        pluginUpdateLog.Add("Error moving file: \t" + destFile);
                    }
                }
            }

            foreach (var directory in sourceDirectoryInfo.GetDirectories())
            {
                string destDir = Path.Combine(targetDirectory, directory.Name);

                if (Directory.Exists(destDir))
                {
                    pluginUpdateLog.Add("Merging directory: \t" + destDir);
                    bool curDirProcessNoErrors = MoveDirectoryFiles(origDirectory, directory.FullName, destDir);

                    if (curDirProcessNoErrors)
                    {
                        Directory.Delete(directory.FullName, true);
                    }

                    noErrors = curDirProcessNoErrors || noErrors;
                }
                else
                {
                    Directory.Move(directory.FullName, destDir);
                    pluginUpdateLog.Add("Moving directory: \t" + destDir);
                }
            }

            return noErrors;
        }

        private void TryLoadDll(string path, string dir)
        {
            if (ProcessFile_Real(path))
            {
                LogMessage("Can't unblock plugin: " + path, 5);
                return;
            }

            var myAsm = Assembly.LoadFrom(path);
            if (myAsm == null)
            {
                return;
            }

            Type[] asmTypes = myAsm.GetTypes();
            if (asmTypes.Length == 0)
            {
                return;
            }

            foreach (var type in asmTypes)
            {
                if (!type.IsSubclassOf(typeof(BasePlugin)))
                {
                    continue;
                }

                var extPlugin = new ExternalPlugin(type, this, dir);
                Plugins.Add(extPlugin.BPlugin);
                LogMessage("Loaded plugin: " + type.Name, 1);
            }
        }

        #region Plugin Methods

        public void InitPlugins()
        {
            ExternalInitialise();
        }

        public void LoadSettings()
        {
            ExternalLoadSettings();
        }

        public void Render()
        {
            ExternalOnRender();
        }

        public void Dispose()
        {
            ExternalClose();
        }

        private void OnEntityAdded(EntityWrapper entityWrapper)
        {
            ExternalEntityAdded(entityWrapper);
        }

        private void OnEntityRemoved(EntityWrapper entityWrapper)
        {
            ExternalEntityRemoved(entityWrapper);
        }

        #endregion
    }
}
