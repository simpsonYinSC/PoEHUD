using System;
using System.Reflection;
using PoEHUD.Plugins;

namespace PoEHUD.HUD.PluginExtension
{
    public sealed class ExternalPlugin
    {
        public BasePlugin BPlugin;
        public string PluginDir; // Will be used for loading resources (images, sounds, etc.) from plugin floder
        private readonly PluginExtensionPlugin api;
        private readonly Type pluginType;
        private object pluginInstance;

        public ExternalPlugin(Type type, PluginExtensionPlugin api, string pluginDir)
        {
            PluginDir = pluginDir;
            pluginType = type;
            this.api = api;
            InitPlugin();
        }

        public string PluginName => pluginType.Name;

        // Also can be used for restarting the plugin
        public void InitPlugin()
        {
            try
            {
                pluginInstance = Activator.CreateInstance(pluginType);
                BPlugin = pluginInstance as BasePlugin;
            }
            catch (Exception e)
            {
                PluginExtensionPlugin.LogError($"Error in plugin constructor! Plugin: {PluginName}, Error: " + e.Message, 5); // TODO: Test this exception
                return;
            }

            if (BPlugin == null)
            {
                return;
            }

            BPlugin.Init(api, this);
            api.ExternalOnRender += BPlugin.ExternalOnRender;
            api.ExternalEntityAdded += BPlugin.ExternalEntityAdded;
            api.ExternalEntityRemoved += BPlugin.ExternalEntityRemoved;
            api.ExternalClose += BPlugin.ExternalOnClose;
            api.ExternalInitialise += BPlugin.ExternalInitialise;
            api.ExternalInitMenu += BPlugin.ExternalInitialiseMenu;
            api.ExternalLoadSettings += BPlugin.ExternalLoadSettings;
        }

        private MethodInfo CheckOverridedMethod(string overrMethodName, string invokeMethodName)
        {
            var overrMethod = pluginType.GetMethod(overrMethodName);

            if (overrMethod == null || overrMethod.DeclaringType == typeof(BasePlugin))
            {
                return null;
            }

            var invokeMethod = typeof(BasePlugin).GetMethod(invokeMethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            if (invokeMethod == null)
            {
                PluginExtensionPlugin.LogError($"Can't find base method {invokeMethodName} in plugin: {PluginName} !", 5);
            }
            else
            {
                return invokeMethod;
            }

            return null;
        }
    }
}
