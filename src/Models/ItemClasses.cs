using System.Collections.Generic;

namespace PoEHUD.Models
{
    public class ItemClasses
    {
        public Dictionary<string, ItemClass> Contents;

        public ItemClasses()
        {
            Contents = new Dictionary<string, ItemClass>
            {
                { "LifeFlask", new ItemClass("Life Flasks", "Flasks") },
                { "ManaFlask", new ItemClass("Mana Flasks", "Flasks") },
                { "HybridFlask", new ItemClass("Hybrid Flasks", "Flasks") },
                { "Currency", new ItemClass("Currency", "Other") },
                { "Amulet", new ItemClass("Amulets", "Jewellery") },
                { "Ring", new ItemClass("Rings", "Jewellery") },
                { "Claw", new ItemClass("Claws", "One Handed Weapon") },
                { "Dagger", new ItemClass("Daggers", "One Handed Weapon") },
                { "Wand", new ItemClass("Wands", "One Handed Weapon") },
                { "One Hand Sword", new ItemClass("One Hand Swords", "One Handed Weapon") },
                { "Thrusting One Hand Sword", new ItemClass("Thrusting One Hand Swords", "One Handed Weapon") },
                { "One Hand Axe", new ItemClass("One Hand Axes", "One Handed Weapon") },
                { "One Hand Mace", new ItemClass("One Hand Maces", "One Handed Weapon") },
                { "Bow", new ItemClass("Bows", "Two Handed Weapon") },
                { "Staff", new ItemClass("Staves", "Two Handed Weapon") },
                { "Two Hand Sword", new ItemClass("Two Hand Swords", "Two Handed Weapon") },
                { "Two Hand Axe", new ItemClass("Two Hand Axes", "Two Handed Weapon") },
                { "Two Hand Mace", new ItemClass("Two Hand Maces", "Two Handed Weapon") },
                { "Active Skill Gem", new ItemClass("Active Skill Gems", "Gems") },
                { "Support Skill Gem", new ItemClass("Support Skill Gems", "Gems") },
                { "Quiver", new ItemClass("Quivers", "Off-hand") },
                { "Belt", new ItemClass("Belts", "Jewellery") },
                { "Gloves", new ItemClass("Gloves", "Armor") },
                { "Boots", new ItemClass("Boots", "Armor") },
                { "Body Armour", new ItemClass("Body Armours", "Armor") },
                { "Helmet", new ItemClass("Helmets", "Armor") },
                { "Shield", new ItemClass("Shields", "Off-hand") },
                { "SmallRelic", new ItemClass("Small Relics", string.Empty) },
                { "MediumRelic", new ItemClass("Medium Relics", string.Empty) },
                { "LargeRelic", new ItemClass("Large Relics", string.Empty) },
                { "StackableCurrency", new ItemClass("Stackable Currency", string.Empty) },
                { "QuestItem", new ItemClass("Quest Items", string.Empty) },
                { "Sceptre", new ItemClass("Sceptres", "One Handed Weapon") },
                { "UtilityFlask", new ItemClass("Utility Flasks", "Flasks") },
                { "UtilityFlaskCritical", new ItemClass("Critical Utility Flasks", string.Empty) },
                { "Map", new ItemClass("Maps", "Other") },
                { "Unarmed", new ItemClass(string.Empty, string.Empty) },
                { "FishingRod", new ItemClass("Fishing Rods", string.Empty) },
                { "MapFragment", new ItemClass("Map Fragments", "Other") },
                { "HideoutDoodad", new ItemClass("Hideout Doodads", string.Empty) },
                { "Microtransaction", new ItemClass("Microtransactions", string.Empty) },
                { "Jewel", new ItemClass("Jewel", "Other") },
                { "DivinationCard", new ItemClass("Divination Card", "Other") },
                { "LabyrinthItem", new ItemClass("Labyrinth Item", string.Empty) },
                { "LabyrinthTrinket", new ItemClass("Labyrinth Trinket", string.Empty) },
                { "LabyrinthMapItem", new ItemClass("Labyrinth Map Item", "Other") },
                { "MiscMapItem", new ItemClass("Misc Map Items", string.Empty) },
                { "Leaguestone", new ItemClass("Leaguestones", "Other") }
            };
        }
    }
}
