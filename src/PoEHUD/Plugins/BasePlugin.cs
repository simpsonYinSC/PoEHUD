using System;
using System.IO;
using PoEHUD.Controllers;
using PoEHUD.Framework;
using PoEHUD.HUD.DebugPlugin;
using PoEHUD.HUD.Menu;
using PoEHUD.HUD.PluginExtension;
using PoEHUD.Models;
using Graphics = PoEHUD.HUD.UI.Graphics;

namespace PoEHUD.Plugins
{
    public abstract class BasePlugin
    {
        public const float PluginErrorDisplayTime = 3;
        public static PluginExtensionPlugin API;
        public string PluginName;
        protected Action externalSaveSettings = delegate { };
        protected Action externalLoadSettings = delegate { };
        private const string LogFileName = "ErrorLog.txt";

        protected BasePlugin()
        {
            PluginName = GetType().Name;
        }

        public GameController GameController => API.GameController;
        public Graphics Graphics => API.Graphics;
        public Memory Memory => GameController.Memory;
        public string PluginDirectory { get; private set; }
        public string LocalPluginDirectory { get; private set; }
        public virtual bool AllowRender => true;
        private string LogPath => PluginDirectory + "\\" + LogFileName;

        public static void LogError(object message, float displayTime)
        {
            LogError(message?.ToString() ?? "null", displayTime);
        }

        public static void LogError(string message, float displayTime)
        {
            PluginExtensionPlugin.LogError(message, displayTime);
        }

        public static void LogMessage(object message, float displayTime)
        {
            LogMessage(message?.ToString() ?? "null", displayTime);
        }

        public static void LogMessage(string message, float displayTime)
        {
            PluginExtensionPlugin.LogMessage(message, displayTime);
        }

        public static void LogMessage(object message, float displayTime, SharpDX.Color color)
        {
            DebugPlugin.LogMessage(message?.ToString() ?? "null", displayTime, color);
        }

        #region ExternalInvokeMethods

        public void ExternalInitialise()
        {
            try
            {
                Initialise();
            }
            catch (Exception e)
            {
                HandlePluginError("Initialise", e);
            }
        }

        public void ExternalOnRender()
        {
            if (!AllowRender)
            {
                return;
            }

            try
            {
                Render();
            }
            catch (Exception e)
            {
                HandlePluginError("Render", e);
            }
        }

        public void ExternalEntityAdded(EntityWrapper entityWrapper)
        {
            try
            {
                EntityAdded(entityWrapper);
            }
            catch (Exception e)
            {
                HandlePluginError("EntityAdded", e);
            }
        }

        public void ExternalEntityRemoved(EntityWrapper entityWrapper)
        {
            try
            {
                EntityRemoved(entityWrapper);
            }
            catch (Exception e)
            {
                HandlePluginError("EntityRemoved", e);
            }
        }

        public void ExternalOnClose()
        {
            try
            {
                OnClose();
            }
            catch (Exception e)
            {
                HandlePluginError("OnClose", e);
            }

            externalSaveSettings();
        }

        public void ExternalInitialiseMenu(MenuItem menu)
        {
            try
            {
                InitialiseMenu(menu);
            }
            catch (Exception e)
            {
                HandlePluginError("InitialiseMenu", e);
            }
        }

        public void ExternalLoadSettings()
        {
            try
            {
                externalLoadSettings();
            }
            catch (Exception e)
            {
                HandlePluginError("LoadSettings", e);
            }
        }

        public void ExternalSaveSetting()
        {
            try
            {
                externalSaveSettings();
            }
            catch (Exception e)
            {
                HandlePluginError("SaveSettings", e);
            }
        }

        #endregion

        public virtual void Initialise()
        {
        }

        public virtual void Render()
        {
        }

        public virtual void EntityAdded(EntityWrapper entityWrapper)
        {
        }

        public virtual void EntityRemoved(EntityWrapper entityWrapper)
        {
        }

        public virtual void OnClose()
        {
        }

        public virtual void InitialiseMenu(MenuItem menu)
        {
        }

        public void Init(PluginExtensionPlugin api, ExternalPlugin pluginData)
        {
            API = api;
            PluginDirectory = pluginData.PluginDir;
            LocalPluginDirectory = PluginDirectory.Substring(PluginDirectory.IndexOf(@"\plugins\", StringComparison.Ordinal) + 1); 
        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }

        private void HandlePluginError(string methodName, Exception exception)
        {
            LogError($"Plugin: '{PluginName}', Error in function: '{methodName}' : '{exception.Message}'", PluginErrorDisplayTime);

            try
            {
                using (StreamWriter w = File.AppendText(LogPath))
                {
                    w.Write("\r\nLog Entry : ");
                    w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                    w.WriteLine($" Method error: {methodName} : {exception}");
                    w.WriteLine("-------------------------------");
                }
            }
            catch (Exception e)
            {
                LogError(" Can't save error log. Error: " + e.Message, 5);
            }
        }
    }
}
