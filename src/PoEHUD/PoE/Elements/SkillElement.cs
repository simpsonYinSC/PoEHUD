namespace PoEHUD.PoE.Elements
{
    public class SkillElement : Element
    {
        public bool IsValid => Unknown1 != 0;

        // Usefull for aura/golums, if they are active or assigned to a key, it's value would be true.
        public bool IsAssignedKeyOrIsActive => Memory.ReadInt(Unknown1 + 0x08) > 3;

        // Couldn't find the skill path, but found skillicon path.
        public string SkillIconPath => Memory.ReadStringU(Memory.ReadLong(Unknown1 + 0x10), 100).TrimEnd('0');

        // Number of time a skill is used ... reset on area change.
        public int TotalUses => Memory.ReadInt(Unknown3 + 0x4C);

        // Usefull for Active Attack skills, movement skills would be true if they are being used. 
        public bool IsUsing => Memory.ReadInt(Unknown3 + 0x08) > 2;

        // A variable is unknown.
        private long Unknown1 => Memory.ReadLong(Address + OffsetBuffers + 0x22C);
        private long Unknown3 => Memory.ReadLong(Address + OffsetBuffers + 0x314);
    }
}
