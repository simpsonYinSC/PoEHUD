using System;
using System.Collections.Generic;
using System.Linq;
using PoEHUD.Controllers;
using PoEHUD.HUD.Settings;
using PoEHUD.HUD.UI;
using PoEHUD.Models;
using SharpDX;
using SharpDX.Direct3D9;

namespace PoEHUD.HUD.DebugPlugin
{
    public class DebugPlugin : SizedPlugin<DebugPluginSettings>
    {
        private static readonly List<string> DebugDrawInfo = new List<string>();
        private static readonly List<DisplayMessage> DebugLog = new List<DisplayMessage>();
        private readonly SettingsHub settingsHub;
        //// private readonly GameController GameController;
        private EntityWrapper lastEntity;

        public DebugPlugin(GameController gameController, Graphics graphics, DebugPluginSettings settings, SettingsHub settingsHub) : base(gameController, graphics, settings)
        {
            this.settingsHub = settingsHub;
            //// GameController = gameController;
        }

        // If delay is -1 message will newer be destroyed
        public static void LogMessage(object o, float delay)
        {
            DebugLog.Add(o == null ? new DisplayMessage("Null", delay, Color.White) : new DisplayMessage(o.ToString(), delay, Color.White));
        }

        public static void LogMessage(object o, float delay, Color color)
        {
            DebugLog.Add(o == null ? new DisplayMessage("Null", delay, color) : new DisplayMessage(o.ToString(), delay, color));
        }

        // Show the message without destroying
        public static void LogInfoMessage(object o)
        {
            DebugDrawInfo.Add(o?.ToString() ?? "Null");
        }

        public override void Render()
        {
            if (DebugDrawInfo.Count == 0 && DebugLog.Count == 0)
            {
                return;
            }

            Vector2 startPosition = StartDrawPointFunc();
            Vector2 position = startPosition;
            int maxWidth = 0;

            position.Y += 10;
            position.X -= 100;

            foreach (string msg in DebugDrawInfo)
            {
                var size = Graphics.DrawText(msg, 15, position, Color.Green, FontDrawFlags.Right);
                position.Y += size.Height;
                maxWidth = Math.Max(size.Width, maxWidth);
            }

            DebugDrawInfo.Clear();
            foreach (var msg in DebugLog.ToList())
            {
                var size = Graphics.DrawText(msg.Message, 15, position, msg.Color, FontDrawFlags.Right);

                position.Y += size.Height;
                maxWidth = Math.Max(size.Width, maxWidth);
                if (msg.Exhaust)
                {
                    DebugLog.Remove(msg);
                }
            }

            if (maxWidth <= 0)
            {
                return;
            }

            var bounds = new RectangleF(startPosition.X - maxWidth - 45, startPosition.Y - 5, maxWidth + 50, position.Y - startPosition.Y + 10);

            Graphics.DrawImage("preload-start.png", bounds, Color.White);
            Graphics.DrawImage("preload-end.png", bounds, Color.White);
            Size = bounds.Size;
            Margin = new Vector2(0, 5);
        }

        protected override void OnEntityAdded(EntityWrapper entityWrapper)
        {
            lastEntity = entityWrapper;
        }

        private void ClearLog()
        {
            DebugLog.Clear();
            DebugDrawInfo.Clear();
        }

        public class DisplayMessage
        {
            public string Message;
            public Color Color;
            private DateTime offTime;

            public DisplayMessage(string message, float delay, Color color)
            {
                Message = message;
                Color = color;

                offTime = delay != -1 ? DateTime.Now.AddSeconds(delay) : DateTime.Now.AddDays(2);
            }

            public bool Exhaust => offTime < DateTime.Now;
        }
    }
}
