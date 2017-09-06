using System;
using System.Collections.Generic;
using System.Media;

namespace PoEHUD.HUD
{
    public static class Sounds
    {
        public static SoundPlayer AlertSound;
        public static SoundPlayer DangerSound;
        public static SoundPlayer TreasureSound;
        public static SoundPlayer AttentionSound;
        private static readonly Dictionary<string, SoundPlayer> SoundLib = new Dictionary<string, SoundPlayer>();

        public static void AddSound(string name)
        {
            if (SoundLib.ContainsKey(name))
            {
                return;
            }

            try
            {
                var soundPlayer = new SoundPlayer($"sounds/{name}");
                soundPlayer.Load();
                SoundLib[name] = soundPlayer;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error when loading {name}| {ex.Message}:", ex);
            }
        }

        public static SoundPlayer GetSound(string name)
        {
            return SoundLib[name];
        }

        public static void LoadSounds()
        {
            AddSound("alert.wav");
            AddSound("danger.wav");
            AddSound("treasure.wav");
            AddSound("attention.wav");
            AlertSound = GetSound("alert.wav");
            DangerSound = GetSound("danger.wav");
            TreasureSound = GetSound("treasure.wav");
            AttentionSound = GetSound("attention.wav");
        }
    }
}
