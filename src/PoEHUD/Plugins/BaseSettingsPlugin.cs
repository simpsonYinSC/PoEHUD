using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using PoEHUD.HUD.Menu;
using PoEHUD.HUD.Settings;

namespace PoEHUD.Plugins
{
    public class BaseSettingsPlugin<TSettings> : BasePlugin where TSettings : SettingsBase, new()
    {
        private const string SettingsFileName = "config.ini";

        public BaseSettingsPlugin()
        {
            externalSaveSettings += SaveSettings;
            externalLoadSettings += LoadSettings;
        }

        public TSettings Settings { get; private set; }
        public MenuItem PluginSettingsRootMenu { get; private set; }
        public override bool AllowRender => Settings.Enable;
        private string SettingsFullPath => PluginDirectory + "\\" + SettingsFileName;

        public override void InitialiseMenu(MenuItem mainMenu)
        {
            PluginSettingsRootMenu = MenuPlugin.AddChild(mainMenu, PluginName, Settings.Enable);
            PropertyInfo[] settingsProps = Settings.GetType().GetProperties();

            Dictionary<int, MenuItem> rootMenu = new Dictionary<int, MenuItem>();

            foreach (var property in settingsProps)
            {
                var menuAttrib = property.GetCustomAttribute<MenuAttribute>();

                MenuItem parentMenu = PluginSettingsRootMenu;

                if (menuAttrib == null)
                {
                    continue;
                }

                if (menuAttrib.ParentIndex != -1)
                {
                    if (rootMenu.ContainsKey(menuAttrib.ParentIndex))
                    {
                        parentMenu = rootMenu[menuAttrib.ParentIndex];
                    }
                    else
                    {
                        LogError($"{PluginName}: Can't find parent menu with index '{menuAttrib.ParentIndex}'!", 5);
                    }
                }

                MenuItem resultItem = null;

                var propType = property.PropertyType;

                if (propType == typeof(ToggleNode) || propType.IsSubclassOf(typeof(ToggleNode)))
                {
                    ToggleNode option = property.GetValue(Settings) as ToggleNode;
                    resultItem = MenuPlugin.AddChild(parentMenu, menuAttrib.MenuName, option);
                }
                else if (propType == typeof(ColorNode) || propType.IsSubclassOf(typeof(ColorNode)))
                {
                    ColorNode option = property.GetValue(Settings) as ColorNode;
                    resultItem = MenuPlugin.AddChild(parentMenu, menuAttrib.MenuName, option);
                }
                else if (propType == typeof(EmptyNode) || propType.IsSubclassOf(typeof(EmptyNode)))
                {
                    resultItem = MenuPlugin.AddChild(parentMenu, menuAttrib.MenuName);
                }
                else if (propType == typeof(HotkeyNode) || propType.IsSubclassOf(typeof(HotkeyNode)))
                {
                    HotkeyNode option = property.GetValue(Settings) as HotkeyNode;
                    resultItem = MenuPlugin.AddChild(parentMenu, menuAttrib.MenuName, option);
                }
                else if (propType == typeof(ButtonNode) || propType.IsSubclassOf(typeof(ButtonNode)))
                {
                    ButtonNode option = property.GetValue(Settings) as ButtonNode;
                    resultItem = MenuPlugin.AddChild(parentMenu, menuAttrib.MenuName, option);
                }
                else if (propType == typeof(ListNode) || propType.IsSubclassOf(typeof(ListNode)))
                {
                    ListNode option = property.GetValue(Settings) as ListNode;
                    var listButton  = MenuPlugin.AddChild(parentMenu, menuAttrib.MenuName, option);
                    resultItem = listButton;
                }
                else if (propType.IsGenericType)
                {
                    // Actually we can use reflection to find correct method in MenuPlugin by argument types and invoke it, but I don't have enough time for this way..
                    /*
                        var method = typeof(MenuPlugin).GetMethods();
                        method.ToList().Find(x => x.Name == "AddChild");
                        */

                    var genericType = propType.GetGenericTypeDefinition();

                    if (genericType == typeof(RangeNode<>))
                    {
                        Type[] genericParameter = propType.GenericTypeArguments;

                        if (genericParameter.Length > 0)
                        {
                            var argType = genericParameter[0];

                            if (argType == typeof(int))
                            {
                                RangeNode<int> option = property.GetValue(Settings) as RangeNode<int>;
                                resultItem = MenuPlugin.AddChild(parentMenu, menuAttrib.MenuName, option);
                            }
                            else if (argType == typeof(float))
                            {
                                RangeNode<float> option = property.GetValue(Settings) as RangeNode<float>;
                                resultItem = MenuPlugin.AddChild(parentMenu, menuAttrib.MenuName, option);
                            }
                            else
                            {
                                LogError($"{PluginName}: Generic node argument '{argType.Name}' is not defined in code. Node type: " + propType.Name, 5);
                            }
                        }
                        else
                        {
                            LogError($"{PluginName}: Can't get GenericTypeArguments from option type: " + propType.Name, 5);
                        }
                    }
                    else
                    {
                        LogError($"{PluginName}: Generic option node is not defined in code: " + genericType?.Name, 5);
                    }
                }
                else
                {
                    LogError($"{PluginName}: Type of option node is not defined: " + propType.Name, 5);
                }

                if (resultItem == null)
                {
                    continue;
                }

                resultItem.TooltipText = menuAttrib.Tooltip;

                if (menuAttrib.Index == -1)
                {
                    continue;
                }

                if (!rootMenu.ContainsKey(menuAttrib.Index))
                {
                    rootMenu.Add(menuAttrib.Index, resultItem);
                }
                else
                {
                    LogError($"{PluginName}: Can't add menu '{menuAttrib.MenuName}', plugin already contains menu with index '{menuAttrib.Index}'!", 5);
                }
            }
        }

        private void LoadSettings()
        {
            try
            {
                string settingsFullPath = SettingsFullPath;

                if (File.Exists(settingsFullPath))
                {
                    string json = File.ReadAllText(settingsFullPath);
                    Settings = JsonConvert.DeserializeObject<TSettings>(json, SettingsHub.JsonSettings);
                }

                // also sometimes config contains only "null" word, so that will be a fix for that
                if (Settings == null) 
                {
                    Settings = new TSettings();
                }
            }
            catch
            {
                LogError($"Plugin {PluginName} error load settings!", 3);
                Settings = new TSettings();
            }

            if (Settings.Enable == null)
            {
                Settings.Enable = false;
            }
        }

        private void SaveSettings()
        {
            try
            {
                string settingsDirName = Path.GetDirectoryName(SettingsFullPath);
                if (!Directory.Exists(settingsDirName))
                {
                    if (settingsDirName != null)
                    {
                        Directory.CreateDirectory(settingsDirName);
                    }
                }

                using (var stream = new StreamWriter(File.Create(SettingsFullPath)))
                {
                    string json = JsonConvert.SerializeObject(Settings, Formatting.Indented, SettingsHub.JsonSettings);
                    stream.Write(json);
                }
            }
            catch
            {
                LogError($"Plugin {PluginName} error save settings!", 3);
            }
        }
    }
}
