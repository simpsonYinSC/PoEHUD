namespace PoEHUD.PoE.Elements
{
    public class EntityLabel : Element
    {
        public string Text
        {
            get
            {
                int labelLen = Memory.ReadInt(Address + 0xC28);
                if (labelLen <= 0 || labelLen > 256)
                {
                    return string.Empty;
                }

                return labelLen >= 8
                    ? Memory.ReadStringU(Memory.ReadInt(Address + 0xC18), labelLen * 2)
                    : Memory.ReadStringU(Address + 0xC18, labelLen * 2);
            }
        }
    }
}
