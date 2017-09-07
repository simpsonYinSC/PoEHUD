namespace PoEHUD.PoE.Elements
{
    public class WindowState : Element
    {
        public new bool IsVisibleLocal => Memory.ReadInt(Address + 0x860) == 1;
    }
}
