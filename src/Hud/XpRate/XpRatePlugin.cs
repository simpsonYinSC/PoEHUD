using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PoEHUD.Controllers;
using PoEHUD.Framework;
using PoEHUD.Framework.Helpers;
using PoEHUD.HUD.Preload;
using PoEHUD.HUD.Settings;
using PoEHUD.HUD.UI;
using PoEHUD.Models;
using PoEHUD.PoE.Components;
using SharpDX;
using SharpDX.Direct3D9;

namespace PoEHUD.HUD.XPRate
{
    public class XPRatePlugin : SizedPlugin<XPRateSettings>
    {
        private readonly SettingsHub settingsHub;
        private string xpRate, timeLeft;
        private DateTime startTime, lastTime;
        private long startXP;
        private double levelXPPenalty, partyXPPenalty;
        private bool holdKey;

        public XPRatePlugin(GameController gameController, Graphics graphics, XPRateSettings settings, SettingsHub settingsHub) : base(gameController, graphics, settings)
        {
            this.settingsHub = settingsHub;
            GameController.Area.AreaChanged += area => AreaChange();
        }

        public override void Render()
        {
            try
            {
                if (!holdKey && WindowsAPI.IsKeyDown(Keys.F10))
                {
                    holdKey = true;
                    Settings.Enable.Value = !Settings.Enable.Value;
                    SettingsHub.Save(settingsHub);
                }
                else if (holdKey && !WindowsAPI.IsKeyDown(Keys.F10))
                {
                    holdKey = false;
                }

                if (!Settings.Enable)
                {
                    return;
                }

                DateTime nowTime = DateTime.Now;
                TimeSpan elapsedTime = nowTime - lastTime;
                if (elapsedTime.TotalSeconds > 1)
                {
                    CalculateXP(nowTime);
                    partyXPPenalty = PartyXPPenalty();
                    lastTime = nowTime;
                }

                bool showInTown = !Settings.ShowInTown && GameController.Area.CurrentArea.IsTown || !Settings.ShowInTown && GameController.Area.CurrentArea.IsHideout;
                Vector2 position = StartDrawPointFunc();
                string fps = $"fps:({GameController.Game.IngameState.CurrentFPS})";
                string areaName = $"{GameController.Area.CurrentArea.DisplayName}";
                Color areaNameColor = PreloadAlertPlugin.AreaNameColor;

                if (Settings.OnlyAreaName)
                {
                    if (!showInTown)
                    {
                        var areaNameSize = Graphics.MeasureText(areaName, Settings.TextSize);
                        float boxHeight = areaNameSize.Height;
                        float boxWidth = MathHepler.Max(areaNameSize.Width);
                        var bounds = new RectangleF(position.X - 84 - boxWidth, position.Y - 5, boxWidth + 90, boxHeight + 12);
                        string latency = $"({GameController.Game.IngameState.CurrentLatency})";
                        Graphics.DrawText(areaName, Settings.TextSize, new Vector2(bounds.X + 84, position.Y), areaNameColor);
                        Graphics.DrawImage("preload-start.png", bounds, Settings.BackgroundColor);
                        Graphics.DrawImage("preload-end.png", bounds, Settings.BackgroundColor);
                        if (Settings.ShowLatency)
                        {
                            Graphics.DrawText(latency, Settings.TextSize, new Vector2(bounds.X + 35, position.Y), Settings.LatencyTextColor);
                        }

                        Size = bounds.Size;
                        Margin = new Vector2(0, 5);
                    }
                }

                if (!Settings.OnlyAreaName && !showInTown)
                {
                    var expReceiving = levelXPPenalty * partyXPPenalty;
                    var expReceivingText = $"{xpRate}  *{expReceiving:p0}";
                    string ping = $"ping:({GameController.Game.IngameState.CurrentLatency})";
                    Size2 areaNameSize = Graphics.DrawText(areaName, Settings.TextSize, position - 1, areaNameColor, FontDrawFlags.Right);
                    Vector2 secondLine = position.Translate(-1, areaNameSize.Height + 2);
                    Size2 expRateSize = Graphics.DrawText(timeLeft, Settings.TextSize, secondLine, Settings.XPHTextColor, FontDrawFlags.Right);
                    Vector2 thirdLine = secondLine.Translate(-1, expRateSize.Height + 2);
                    Size2 expLeftSize = Graphics.DrawText(expReceivingText, Settings.TextSize, thirdLine, Settings.TimeLeftColor, FontDrawFlags.Right);
                    string timer = AreaInstance.GetTimeString(nowTime - GameController.Area.CurrentArea.TimeEntered);
                    Size2 timerSize = Graphics.MeasureText(timer, Settings.TextSize);

                    float boxWidth = MathHepler.Max(expRateSize.Width + 40, expLeftSize.Width + 40, areaNameSize.Width + 20, timerSize.Width);
                    float boxHeight = expRateSize.Height + expLeftSize.Height + areaNameSize.Height;
                    var bounds = new RectangleF(position.X - boxWidth - 104, position.Y - 7, boxWidth + 110, boxHeight + 18);

                    Size2 timeFpsSize = Graphics.MeasureText(fps, Settings.TextSize);
                    var dif = bounds.Width - (12 + timeFpsSize.Width + expRateSize.Width);
                    if (dif < 0)
                    {
                        bounds.X += dif;
                        bounds.Width -= dif;
                    }

                    Graphics.DrawText(timer, Settings.TextSize, new Vector2(bounds.X + 70, position.Y), Settings.TimerTextColor);
                    Graphics.DrawText(fps, Settings.TextSize, new Vector2(bounds.X + 70, secondLine.Y), Settings.FPSTextColor);
                    Graphics.DrawText(ping, Settings.TextSize, new Vector2(bounds.X + 70, thirdLine.Y), Settings.LatencyTextColor);
                    Graphics.DrawImage("preload-start.png", bounds, Settings.BackgroundColor);
                    Graphics.DrawImage("preload-end.png", bounds, Settings.BackgroundColor);
                    Size = bounds.Size;
                    Margin = new Vector2(0, 5);
                }
            }
            catch
            {
                // ignore
            }
        }

        private void CalculateXP(DateTime nowTime)
        {
            int level = GameController.Player.GetComponent<Player>().Level;
            if (level >= 100)
            {
                // player can't level up, just show fillers
                xpRate = "0.00 xp/h";
                timeLeft = "--h--m--s";
                return;
            }

            long currentXP = GameController.Player.GetComponent<Player>().XP;
            double rate = (currentXP - startXP) / (nowTime - startTime).TotalHours;
            xpRate = $"{ConvertHelper.ToShorten(rate, "0.00")} xp/h";
            if (level < 0 || level + 1 >= Constants.PlayerXPLevels.Length || !(rate > 1))
            {
                return;
            }

            long expLeft = Constants.PlayerXPLevels[level + 1] - currentXP;
            TimeSpan time = TimeSpan.FromHours(expLeft / rate);
            timeLeft = $"{time.Hours:0}h {time.Minutes:00}m {time.Seconds:00}s to level up";
        }

        private double LevelXPPenalty()
        {
            int arenaLevel = GameController.Area.CurrentArea.RealLevel;
            int characterLevel = GameController.Player.GetComponent<Player>().Level;
            double safeZone = Math.Floor(Convert.ToDouble(characterLevel) / 16) + 3;
            double effectiveDifference = Math.Max(Math.Abs(characterLevel - arenaLevel) - safeZone, 0);
            double expMultiplier = Math.Max(Math.Pow((characterLevel + 5) / (characterLevel + 5 + Math.Pow(effectiveDifference, 2.5)), 1.5), 0.01);
            return expMultiplier;
        }

        private double PartyXPPenalty()
        {
            List<int> levels = GameController.Entities.Where(y => y.HasComponent<Player>()).Select(y => y.GetComponent<Player>().Level).ToList();
            int characterLevel = GameController.Player.GetComponent<Player>().Level;
            return (partyXPPenalty = Math.Pow(characterLevel + 10, 2.71) / levels.Sum(level => Math.Pow(level + 10, 2.71))) * levels.Count;
        }

        private void AreaChange()
        {
            if (GameController.InGame)
            {
                startXP = GameController.Player.GetComponent<Player>().XP;
                levelXPPenalty = LevelXPPenalty();
            }

            startTime = lastTime = DateTime.Now;
            xpRate = "0.00 xp/h";
            timeLeft = "-h -m -s  to level up";
        }
    }
}
