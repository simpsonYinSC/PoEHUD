using System;
using PoEHUD.Models.Enums;
using PoEHUD.PoE;
using PoEHUD.PoE.Components;
using PoEHUD.PoE.RemoteMemoryObjects;

namespace PoEHUD.Models
{
    public sealed class ItemStats
    {
        private static StatTranslator translate;
        private readonly Entity item;
        private readonly float[] stats;

        public ItemStats(Entity item)
        {
            this.item = item;
            if (translate == null)
            {
                translate = new StatTranslator();
            }

            stats = new float[Enum.GetValues(typeof(ItemStatEnum)).Length];
            ParseSockets();
            ParseExplicitMods();
            if (item.HasComponent<Weapon>())
            {
                ParseWeaponStats();
            }
        }

        public void AddToMod(ItemStatEnum stat, float value)
        {
            stats[(int)stat] += value;
        }

        public float GetStat(ItemStatEnum stat)
        {
            return stats[(int)stat];
        }

        private void ParseWeaponStats()
        {
            var component = item.GetComponent<Weapon>();
            float num = (component.DamageMinimum + component.DamageMaximum) / 2f + GetStat(ItemStatEnum.LocalPhysicalDamage);
            num *= 1f + (GetStat(ItemStatEnum.LocalPhysicalDamagePercent) + item.GetComponent<Quality>().ItemQuality) / 100f;
            AddToMod(ItemStatEnum.AveragePhysicalDamage, num);
            float num2 = 1f / (component.AttackTime / 1000f);
            num2 *= 1f + GetStat(ItemStatEnum.LocalAttackSpeed) / 100f;
            AddToMod(ItemStatEnum.AttackPerSecond, num2);
            float num3 = component.CriticalChance / 100f;
            num3 *= 1f + GetStat(ItemStatEnum.LocalCritChance) / 100f;
            AddToMod(ItemStatEnum.WeaponCritChance, num3);
            float num4 = GetStat(ItemStatEnum.LocalAddedColdDamage) + GetStat(ItemStatEnum.LocalAddedFireDamage) + GetStat(ItemStatEnum.LocalAddedLightningDamage);
            AddToMod(ItemStatEnum.AverageElementalDamage, num4);
            AddToMod(ItemStatEnum.DPS, (num + num4) * num2);
            AddToMod(ItemStatEnum.PhysicalDPS, num * num2);
        }

        private void ParseExplicitMods()
        {
            foreach (ItemMod current in item.GetComponent<Mods>().ItemMods)
            {
                translate.Translate(this, current);
            }

            AddToMod(ItemStatEnum.ElementalResistance, GetStat(ItemStatEnum.LightningResistance) + GetStat(ItemStatEnum.FireResistance) + GetStat(ItemStatEnum.ColdResistance));
            AddToMod(ItemStatEnum.TotalResistance, GetStat(ItemStatEnum.ElementalResistance) + GetStat(ItemStatEnum.TotalResistance));
        }

        private void ParseSockets()
        {
            // TODO: ParseSockets do nothing
        }
    }
}
