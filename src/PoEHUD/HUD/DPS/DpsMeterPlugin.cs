using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PoEHUD.Controllers;
using PoEHUD.Framework;
using PoEHUD.Framework.Helpers;
using PoEHUD.HUD.UI;
using PoEHUD.Models;
using PoEHUD.PoE.Components;
using SharpDX;
using SharpDX.Direct3D9;

namespace PoEHUD.HUD.DPS
{
    public class DPSMeterPlugin : SizedPlugin<DPSMeterSettings>
    {
        private const double DPSPeriod = 0.2;
        private readonly Dictionary<long, int> lastMonsters = new Dictionary<long, int>();
        private DateTime lastTime;
        private double[] damageMemory = new double[10];
        private int damageMemoryIndex;
        private int maxDPS;

        public DPSMeterPlugin(GameController gameController, Graphics graphics, DPSMeterSettings settings) : base(gameController, graphics, settings)
        {
            lastTime = DateTime.Now;
            GameController.Area.AreaChanged += area =>
            {
                lastTime = DateTime.Now;
                maxDPS = 0;
                damageMemory = new double[10];
                lastMonsters.Clear();
            };
        }

        public override void Render()
        {
            try
            {
                base.Render();
                if (!Settings.Enable || WindowsAPI.IsKeyDown(Keys.F10) || !Settings.ShowInTown && GameController.Area.CurrentArea.IsTown || !Settings.ShowInTown && GameController.Area.CurrentArea.IsHideout)
                {
                    return;
                }

                DateTime nowTime = DateTime.Now;
                TimeSpan elapsedTime = nowTime - lastTime;
                if (elapsedTime.TotalSeconds > DPSPeriod)
                {
                    damageMemoryIndex++;
                    if (damageMemoryIndex >= damageMemory.Length)
                    {
                        damageMemoryIndex = 0;
                    }

                    damageMemory[damageMemoryIndex] = CalculateDPS();
                    lastTime = nowTime;
                }

                Vector2 position = StartDrawPointFunc();
                var dps = (int)damageMemory.Sum();
                maxDPS = Math.Max(dps, maxDPS);

                string dpsText = dps + " dps";
                string peakText = maxDPS + " top dps";
                Size2 dpsSize = Graphics.DrawText(dpsText, Settings.DPSTextSize, position, Settings.DPSFontColor, FontDrawFlags.Right);
                Size2 peakSize = Graphics.DrawText(peakText, Settings.PeakDPSTextSize, position.Translate(0, dpsSize.Height), Settings.PeakFontColor, FontDrawFlags.Right);

                int width = Math.Max(peakSize.Width, dpsSize.Width);
                int height = dpsSize.Height + peakSize.Height;
                var bounds = new RectangleF(position.X - 5 - width - 41, position.Y - 5, width + 50, height + 10);

                Graphics.DrawImage("preload-start.png", bounds, Settings.BackgroundColor);
                Graphics.DrawImage("preload-end.png", bounds, Settings.BackgroundColor);

                Size = bounds.Size;
                Margin = new Vector2(0, 5);
            }
            catch
            {
                // ignore
            }
        }

        private double CalculateDPS()
        {
            int totalDamage = 0;
            foreach (EntityWrapper monster in GameController.Entities.Where(x => x.HasComponent<Monster>() && x.IsHostile))
            {
                var life = monster.GetComponent<Life>();
                int hp = monster.IsAlive ? life.CurrentHP + life.CurrentES : 0;
                if (hp <= -1000000 || hp >= 10000000)
                {
                    continue;
                }

                if (lastMonsters.TryGetValue(monster.Id, out int lastHP))
                {
                    if (lastHP != hp)
                    {
                        totalDamage += lastHP - hp;
                    }
                }

                lastMonsters[monster.Id] = hp;
            }

            return totalDamage < 0 ? 0 : totalDamage;
        }
    }
}
