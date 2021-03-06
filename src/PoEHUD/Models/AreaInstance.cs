using System;
using PoEHUD.PoE.RemoteMemoryObjects;

namespace PoEHUD.Models
{
    public sealed class AreaInstance
    {
        public DateTime TimeEntered = DateTime.Now;

        public AreaInstance(AreaTemplate area, int hash, int realLevel)
        {
            Hash = hash;
            RealLevel = realLevel;
            NominalLevel = area.NominalLevel;
            Name = area.Name;
            Act = area.Act;
            IsTown = area.IsTown;
            HasWaypoint = area.HasWaypoint;
            IsHideout = Name.Contains("Hideout");
        }

        public int RealLevel { get; }
        public int NominalLevel { get; }
        public string Name { get; }
        public int Act { get; }
        public bool IsTown { get; }
        public bool IsHideout { get; }
        public bool HasWaypoint { get; }
        public int Hash { get; }
        public string DisplayName => string.Concat(Name, " (", RealLevel, ")");

        public static string GetTimeString(TimeSpan timeSpent)
        {
            int allsec = (int)timeSpent.TotalSeconds;
            int secs = allsec % 60;
            int mins = allsec / 60;
            int hours = mins / 60;
            mins = mins % 60;
            return string.Format(hours > 0 ? "{0}:{1:00}:{2:00}" : "{1}:{2:00}", hours, mins, secs);
        }

        public override string ToString()
        {
            return $"{Name} ({RealLevel}) #{Hash}";
        }
    }
}
