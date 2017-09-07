using System.Collections.Generic;
using System.Diagnostics;
using PoEHUD.Models;
using PoEHUD.Models.Enums;
using PoEHUD.PoE.Components;

namespace PoEHUD.HUD.Health
{
    public class HealthBar
    {
        private const int DPSCheckTime = 1000;
        private const int DPSFastCheckTime = 200;
        private const int DPSPopTime = 2000;
        private readonly Stopwatch pdsStopwatch = Stopwatch.StartNew();
        private readonly bool isHostile;
        private int lastHP;

        public HealthBar(EntityWrapper entity, HealthBarSettings settings)
        {
            Entity = entity;
            if (entity.Path.Contains("GoddessOfJustice"))
            {
                return;
            }

            if (entity.HasComponent<Player>())
            {
                Type = CreatureType.Player;
                Settings = settings.Players;
                IsValid = true;
            }
            else if (entity.HasComponent<Monster>())
            {
                IsValid = true;
                if (entity.IsHostile)
                {
                    isHostile = true;
                    switch (entity.GetComponent<ObjectMagicProperties>().Rarity)
                    {
                        case MonsterRarity.White:
                            Type = CreatureType.Normal;
                            Settings = settings.NormalEnemy;
                            break;
                        case MonsterRarity.Magic:
                            Type = CreatureType.Magic;
                            Settings = settings.MagicEnemy;
                            break;
                        case MonsterRarity.Rare:
                            Settings = settings.RareEnemy;
                            Type = CreatureType.Rare;
                            break;
                        case MonsterRarity.Unique:
                            Settings = settings.UniqueEnemy;
                            Type = CreatureType.Unique;
                            break;
                    }
                }
                else
                {
                    Type = CreatureType.Minion;
                    Settings = settings.Minions;
                }
            }

            Life = Entity.GetComponent<Life>();
            lastHP = GetFullHP();
        }

        public Life Life { get; }
        public EntityWrapper Entity { get; }
        public bool IsValid { get; private set; }
        public UnitSettings Settings { get; }
        public CreatureType Type { get; private set; }

        public LinkedList<int> DPSQueue { get; } = new LinkedList<int>();

        public bool IsShow(bool showEnemy)
        {
            return !isHostile ? Settings.Enable.Value : Settings.Enable && showEnemy && isHostile;
        }

        public void DPSRefresh()
        {
            int chechTime = DPSQueue.Count > 0 ? DPSCheckTime : DPSFastCheckTime;
            if (pdsStopwatch.ElapsedMilliseconds < chechTime)
            {
                return;
            }

            int hp = GetFullHP();
            if (hp <= -1000000 || hp >= 10000000 || lastHP == hp)
            {
                return;
            }

            DPSQueue.AddFirst(-(lastHP - hp));
            if (DPSQueue.Count > Settings.FloatingCombatStackSize)
            {
                DPSQueue.RemoveLast();
                pdsStopwatch.Restart();
            }

            lastHP = hp;
        }

        public void DPSDequeue()
        {
            if (pdsStopwatch.ElapsedMilliseconds < DPSPopTime)
            {
                return;
            }

            if (DPSQueue.Count > 0)
            {
                DPSQueue.RemoveLast();
            }

            pdsStopwatch.Restart();
        }

        private int GetFullHP()
        {
            return Life.CurrentHP + Life.CurrentES;
        }
    }
}
