using System;
using PoEHUD.Models.Enums;

namespace PoEHUD.PoE.RemoteMemoryObjects
{
    public class IngameState : RemoteMemoryObject
    {
        public Camera Camera => GetObject<Camera>(Address + 0x1704 + Offset.IgsOffsetDelta);
        public IngameData Data => ReadObject<IngameData>(Address + 0x170 + Offset.IgsOffset);
        public bool InGame => ServerData.IsInGame;
        public ServerData ServerData => ReadObjectAt<ServerData>(0x178 + Offset.IgsOffset);
        public IngameUIElements IngameUI => ReadObjectAt<IngameUIElements>(0x5D0 + Offset.IgsOffset);
        public Element UIRoot => ReadObjectAt<Element>(0xC80 + Offset.IgsOffset);
        public Element UIHover => ReadObjectAt<Element>(0xCA8 + Offset.IgsOffset);
        public float CurrentUIElementPositionX => Memory.ReadFloat(Address + 0xCB0 + Offset.IgsOffset);
        public float CurrentUIElementPositionY => Memory.ReadFloat(Address + 0xCB4 + Offset.IgsOffset);
        public long EntityLabelMap => Memory.ReadLong(Address + 0x98, 0xA70);
        public DiagnosticInfoType DiagnosticInfoType => (DiagnosticInfoType)Memory.ReadInt(Address + 0xD38 + Offset.IgsOffset);
        public DiagnosticElement LatencyRectangle => GetObjectAt<DiagnosticElement>(0xF68 + Offset.IgsOffset);
        public DiagnosticElement FrameTimeRectangle => GetObjectAt<DiagnosticElement>(0x13F8 + Offset.IgsOffset);
        public DiagnosticElement FPSRectangle => GetObjectAt<DiagnosticElement>(0x1640 + Offset.IgsOffset);
        public float CurrentLatency => LatencyRectangle.CurrentValue;
        public float CurrentFrameTime => FrameTimeRectangle.CurrentValue;
        public float CurrentFPS => FPSRectangle.CurrentValue;
        public TimeSpan TimeInGame => TimeSpan.FromSeconds(Memory.ReadFloat(Address + 0xD1C + Offset.IgsOffset));
        public float TimeInGameFloat => Memory.ReadFloat(Address + 0xD20 + Offset.IgsOffset);
    }
}
