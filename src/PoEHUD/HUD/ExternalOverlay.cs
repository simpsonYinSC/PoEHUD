using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PoEHUD.Controllers;
using PoEHUD.Framework;
using PoEHUD.Framework.Helpers;
using PoEHUD.HUD.AdvancedTooltip;
using PoEHUD.HUD.DebugPlugin;
using PoEHUD.HUD.DPS;
using PoEHUD.HUD.Health;
using PoEHUD.HUD.Icons;
using PoEHUD.HUD.Interfaces;
using PoEHUD.HUD.KillCounter;
using PoEHUD.HUD.Loot;
using PoEHUD.HUD.Menu;
using PoEHUD.HUD.PluginExtension;
using PoEHUD.HUD.Preload;
using PoEHUD.HUD.Settings;
using PoEHUD.HUD.Trackers;
using PoEHUD.HUD.UI;
using PoEHUD.HUD.XPRate;
using PoEHUD.Models.Enums;
using PoEHUD.PoE;
using SharpDX;
using SharpDX.Windows;
using Color = System.Drawing.Color;
using Graphics2D = PoEHUD.HUD.UI.Graphics;
using Rectangle = System.Drawing.Rectangle;

namespace PoEHUD.HUD
{
    internal sealed class ExternalOverlay : RenderForm
    {
        private readonly SettingsHub settings;
        private readonly GameController gameController;
        private readonly Func<bool> gameEnded;
        private readonly IntPtr gameHandle;
        private readonly List<IPlugin> plugins = new List<IPlugin>();
        private Graphics graphics;

        public ExternalOverlay(GameController gameController, Func<bool> gameEnded)
        {
            settings = SettingsHub.Load();
            this.gameController = gameController;
            this.gameEnded = gameEnded;
            gameHandle = gameController.Window.Process.MainWindowHandle;
            SuspendLayout();
            Text = MathHepler.GetRandomWord(MathHepler.Randomizer.Next(7) + 5);
            TransparencyKey = Color.Transparent;
            BackColor = Color.Black;
            FormBorderStyle = FormBorderStyle.None;
            ShowIcon = false;
            TopMost = true;
            ResumeLayout(false);
            Load += OnLoad;
        }

        private async void CheckGameState()
        {
            while (!gameEnded())
            {
                await Task.Delay(500);
            }

            graphics.Dispose();
            Close();
        }

        private async void CheckGameWindow()
        {
            while (!gameEnded())
            {
                await Task.Delay(1000);
                Rectangle gameSize = WindowsAPI.GetClientRectangle(gameHandle);
                Bounds = gameSize;
            }
        }

        private IEnumerable<MapIcon> GatherMapIcons()
        {
            IEnumerable<IPluginWithMapIcons> pluginsWithIcons = plugins.OfType<IPluginWithMapIcons>();
            return pluginsWithIcons.SelectMany(iconSource => iconSource.GetIcons());
        }

        private Vector2 GetLeftCornerMap()
        {
            var ingameState = gameController.Game.IngameState;
            RectangleF clientRect = ingameState.IngameUI.Map.SmallMinimap.GetClientRect();
            var diagnosticElement = ingameState.LatencyRectangle;
            switch (ingameState.DiagnosticInfoType)
            {
                case DiagnosticInfoType.Short:
                    clientRect.X = diagnosticElement.X + 30;
                    break;

                case DiagnosticInfoType.Full:
                    clientRect.Y = diagnosticElement.Y + diagnosticElement.Height + 5;
                    var fpsRectangle = ingameState.FPSRectangle;
                    clientRect.X = fpsRectangle.X + fpsRectangle.Width + 6;
                    break;
            }

            return new Vector2(clientRect.X - 5, clientRect.Y + 5);
        }

        private Vector2 GetUnderCornerMap()
        {
            const int epsilon = 1;
            Element questPanel = gameController.Game.IngameState.IngameUI.QuestTracker;
            Element gemPanel = gameController.Game.IngameState.IngameUI.GemLevelUpPanel;
            RectangleF questPanelRect = questPanel.GetClientRect();
            RectangleF gemPanelRect = gemPanel.GetClientRect();
            RectangleF clientRect = gameController.Game.IngameState.IngameUI.Map.SmallMinimap.GetClientRect();
            if (gemPanel.IsVisible && Math.Abs(gemPanelRect.Right - clientRect.Right) < epsilon)
            {
                // gem panel is visible, add its height
                clientRect.Height += gemPanelRect.Height;
            }

            if (questPanel.IsVisible && Math.Abs(gemPanelRect.Right - clientRect.Right) < epsilon)
            {
                // quest panel is visible, add its height
                clientRect.Height += questPanelRect.Height;
            }

            return new Vector2(clientRect.X + clientRect.Width, clientRect.Y + clientRect.Height + 10);
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            SettingsHub.Save(settings);
            plugins.ForEach(plugin => plugin.Dispose());
            graphics.Dispose();  
        }

        private void OnDeactivate(object sender, EventArgs e)
        {
            BringToFront();
        }

        private async void OnLoad(object sender, EventArgs e)
        {
            Bounds = WindowsAPI.GetClientRectangle(gameHandle);
            WindowsAPI.EnableTransparent(Handle, Bounds);
            graphics = new Graphics(this, Bounds.Width, Bounds.Height);

            plugins.Add(new HealthBarPlugin(gameController, graphics, settings.HealthBarSettings));
            plugins.Add(new MinimapPlugin(gameController, graphics, GatherMapIcons, settings.MapIconsSettings));
            plugins.Add(new LargeMapPlugin(gameController, graphics, GatherMapIcons, settings.MapIconsSettings));
            plugins.Add(new MonsterTracker(gameController, graphics, settings.MonsterTrackerSettings));
            plugins.Add(new PoITracker(gameController, graphics, settings.PoITrackerSettings));

            var leftPanel = new PluginPanel(GetLeftCornerMap);
            leftPanel.AddChildren(new XPRatePlugin(gameController, graphics, settings.XPRateSettings, settings));
            leftPanel.AddChildren(new PreloadAlertPlugin(gameController, graphics, settings.PreloadAlertSettings, settings));
            leftPanel.AddChildren(new KillCounterPlugin(gameController, graphics, settings.KillCounterSettings));
            leftPanel.AddChildren(new DPSMeterPlugin(gameController, graphics, settings.DPSMeterSettings));
            leftPanel.AddChildren(new DebugPlugin.DebugPlugin(gameController, graphics, new DebugPluginSettings(), settings));

            var horizontalPanel = new PluginPanel(Direction.Left);
            leftPanel.AddChildren(horizontalPanel);
            plugins.AddRange(leftPanel.GetPlugins());

            var underPanel = new PluginPanel(GetUnderCornerMap);
            underPanel.AddChildren(new ItemAlertPlugin(gameController, graphics, settings.ItemAlertSettings, settings));
            plugins.AddRange(underPanel.GetPlugins());

            plugins.Add(new AdvancedTooltipPlugin(gameController, graphics, settings.AdvancedTooltipSettings, settings));
            plugins.Add(new MenuPlugin(gameController, graphics, settings));
            plugins.Add(new PluginExtensionPlugin(gameController, graphics)); // Should be after MenuPlugin

            Deactivate += OnDeactivate;
            FormClosing += OnClosing;

            CheckGameWindow();
            CheckGameState();
            graphics.OnRender += OnRender;
            await Task.Run(() => graphics.RenderLoop());
        }
        
        private void OnRender()
        {
            if (!gameController.InGame
                || !WindowsAPI.IsForegroundWindow(gameHandle)
                || gameController.Game.IngameState.IngameUI.TreePanel.IsVisible
                || gameController.Game.IngameState.IngameUI.AtlasPanel.IsVisible)
            {
                return;
            }

            gameController.RefreshState();
            plugins.ForEach(x => x.Render());
        }
    }
}
