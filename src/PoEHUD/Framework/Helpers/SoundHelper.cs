using System;
using System.Media;

namespace PoEHUD.Framework.Helpers
{
    public static class SoundHelper
    {
        public static void Play(this SoundPlayer player, int volume)
        {
            const ushort maxVolume = 100;
            ushort newVolume = (ushort) ((float) volume / maxVolume * ushort.MaxValue);
            uint stereo = newVolume | (uint) newVolume << 16;
            WindowsAPI.waveOutSetVolume(IntPtr.Zero, stereo);
            player.Play();
        }
    }
}
