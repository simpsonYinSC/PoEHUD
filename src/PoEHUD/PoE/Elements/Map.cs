namespace PoEHUD.PoE.Elements
{
    public class Map : Element
    {
        //// public Element MapProperties => ReadObjectAt<Element>(0x1FC + OffsetBuffers);
        public Element LargeMap => ReadObjectAt<Element>(0x314 + OffsetBuffers);
        public float LargeMapShiftX => Memory.ReadFloat(LargeMap.Address + OffsetBuffers + 0x2A4);
        public float LargeMapShiftY => Memory.ReadFloat(LargeMap.Address + OffsetBuffers + 0x2A8);
        public float LargeMapZoom => Memory.ReadFloat(LargeMap.Address + OffsetBuffers + 0x2E8);

        public Element SmallMinimap => ReadObjectAt<Element>(0x31C + OffsetBuffers);
        public float SmallMinimapX => Memory.ReadFloat(SmallMinimap.Address + OffsetBuffers + 0x2A4);
        public float SmallMinimapY => Memory.ReadFloat(SmallMinimap.Address + OffsetBuffers + 0x2A8);
        public float SmallMinimapZoom => Memory.ReadFloat(SmallMinimap.Address + OffsetBuffers + 0x2E8);

        public Element OrangeWords => ReadObjectAt<Element>(0x334 + OffsetBuffers);
        public Element BlueWords => ReadObjectAt<Element>(0x36C + OffsetBuffers);
    }
}
