using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PoEHUD.Controllers;
using PoEHUD.Framework;
using PoEHUD.Framework.Helpers;
using PoEHUD.HUD.UI;
using PoEHUD.Models;
using PoEHUD.Models.Enums;
using PoEHUD.Models.Interfaces;
using PoEHUD.PoE.Components;
using SharpDX;
using SharpDX.Direct3D9;

namespace PoEHUD.HUD.KillCounter
{
    public class KillCounterPlugin : SizedPlugin<KillCounterSettings>
    {
        private readonly HashSet<EntityWrapper> aliveEntities;
        private readonly Dictionary<int, HashSet<long>> countedIds;
        private readonly GameController gameController;
        private readonly Dictionary<MonsterRarity, int> counters;
        private int summaryCounter;
        private int sessionCounter;

        public KillCounterPlugin(GameController gameController, Graphics graphics, KillCounterSettings settings)
            : base(gameController, graphics, settings)
        {
            this.gameController = gameController;
            aliveEntities = new HashSet<EntityWrapper>();
            countedIds = new Dictionary<int, HashSet<long>>();
            counters = new Dictionary<MonsterRarity, int>();
            Init();
            GameController.Area.AreaChanged += area =>
            {
                if (!Settings.Enable)
                {
                    return;
                }

                aliveEntities.Clear();
                countedIds.Clear();
                counters.Clear();
                sessionCounter += summaryCounter;
                summaryCounter = 0;
                Init();
            };
        }

        public override void Render()
        {
            try
            {
                base.Render();
                if (!Settings.Enable || WindowsAPI.IsKeyDown(Keys.F10)
                    || !Settings.ShowInTown && GameController.Area.CurrentArea.IsTown
                    || !Settings.ShowInTown && GameController.Area.CurrentArea.IsHideout)
                {
                    return;
                }

                List<EntityWrapper> deadEntities = aliveEntities.Where(entity => !entity.IsAlive).ToList();
                foreach (EntityWrapper entity in deadEntities)
                {
                    Calc(entity);
                    aliveEntities.Remove(entity);
                }

                Vector2 position = StartDrawPointFunc();
                var size = new Size2();
                if (Settings.ShowDetail)
                {
                    size = DrawCounters(position);
                }

                string session = $"({sessionCounter + summaryCounter})";
                Size2 size2 = Graphics.DrawText($"kills: {summaryCounter} {session}", Settings.KillsTextSize, position.Translate(0, size.Height), Settings.TextColor, FontDrawFlags.Right);
                int width = Math.Max(size.Width, size2.Width);
                var bounds = new RectangleF(position.X - width - 46, position.Y - 5, width + 50, size.Height + size2.Height + 10);
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

        protected override void OnEntityAdded(EntityWrapper entityWrapper)
        {
            if (!Settings.Enable)
            {
                return;
            }

            if (!entityWrapper.HasComponent<Monster>())
            {
                return;
            }

            if (entityWrapper.IsAlive)
            {
                aliveEntities.Add(entityWrapper);
            }
            else
            {
                Calc(entityWrapper);
            }
        }

        protected override void OnEntityRemoved(EntityWrapper entityWrapper)
        {
            if (aliveEntities.Contains(entityWrapper))
            {
                aliveEntities.Remove(entityWrapper);
            }
        }

        private void Calc(IEntity entityWrapper)
        {
            HashSet<long> monstersHashSet;
            int areaHash = gameController.Area.CurrentArea.Hash;

            if (!countedIds.TryGetValue(areaHash, out monstersHashSet))
            {
                monstersHashSet = new HashSet<long>();
                countedIds[areaHash] = monstersHashSet;
            }

            if (monstersHashSet.Contains(entityWrapper.Id))
            {
                return;
            }

            monstersHashSet.Add(entityWrapper.Id);
            MonsterRarity rarity = entityWrapper.GetComponent<ObjectMagicProperties>().Rarity;
            if (!entityWrapper.IsHostile || !counters.ContainsKey(rarity))
            {
                return;
            }

            counters[rarity]++;
            summaryCounter++;
        }

        private Size2 DrawCounter(Vector2 position, string label, string counterValue, Color color)
        {
            Size2 measuredSize1 = Graphics.MeasureText(counterValue, 16, FontDrawFlags.Right);
            Size2 measuredSize2 = Graphics.MeasureText(label, 11, FontDrawFlags.Right);
            if (measuredSize1.Width > measuredSize2.Width)
            {
                Size2 size = Graphics.DrawText(counterValue, 16, position, color, FontDrawFlags.Right);
                Size2 size2 = Graphics.DrawText(label, 11, position.Translate(-size.Width / 2f, size.Height), Color.White, FontDrawFlags.Center);
                return new Size2(size.Width, size.Height + size2.Height);
            }
            else
            {
                Size2 size2 = Graphics.DrawText(label, 11, position.Translate(0, measuredSize1.Height), Color.White, FontDrawFlags.Right);
                Size2 size = Graphics.DrawText(counterValue, 16, position.Translate(-size2.Width / 2f, 0), color, FontDrawFlags.Center);
                return new Size2(size2.Width, size.Height + size2.Height);
            }
        }

        private Size2 DrawCounters(Vector2 position)
        {
            const int innerMargin = 15;
            Size2 size = DrawCounter(position, string.Empty, counters[MonsterRarity.White].ToString(), Color.White);
            size = new Size2(DrawCounter(position.Translate(-size.Width - innerMargin, 0), string.Empty, counters[MonsterRarity.Magic].ToString(), HUDSkin.MagicColor).Width + size.Width + innerMargin, size.Height);
            size = new Size2(DrawCounter(position.Translate(-size.Width - innerMargin, 0), string.Empty, counters[MonsterRarity.Rare].ToString(), HUDSkin.RareColor).Width + size.Width + innerMargin, size.Height);
            size = new Size2(DrawCounter(position.Translate(-size.Width - innerMargin, 0), string.Empty, counters[MonsterRarity.Unique].ToString(), HUDSkin.UniqueColor).Width + size.Width + innerMargin, size.Height);
            return size;
        }

        private void Init()
        {
            foreach (MonsterRarity rarity in Enum.GetValues(typeof(MonsterRarity)))
            {
                counters[rarity] = 0;
            }
        }
    }
}
