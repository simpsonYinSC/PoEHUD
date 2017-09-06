using System;
using System.Windows.Forms;
using Newtonsoft.Json;
using PoEHUD.Framework;

namespace PoEHUD.HUD.Settings
{
    public class HotkeyNode
    {
        [JsonIgnore]
        public Action OnValueChanged = delegate { };

        private Keys value;
        private bool pressed;

        public HotkeyNode()
        {
            value = Keys.Space;
        }

        public HotkeyNode(Keys value)
        {
            Value = value;
        }

        public Keys Value
        {
            get => value;
            set
            {
                if (this.value == value)
                {
                    return;
                }

                this.value = value;
                
                try
                {
                    OnValueChanged();
                }
                catch
                {
                    DebugPlugin.DebugPlugin.LogMessage("Error in function that subscribed for: HotkeyNode.OnValueChanged", 10, SharpDX.Color.Red);
                }
            }
        }

        public static implicit operator Keys(HotkeyNode node)
        {
            return node.Value;
        }

        public static implicit operator HotkeyNode(Keys value)
        {
            return new HotkeyNode(value);
        }

        public bool PressedOnce()
        {
            if (WindowsAPI.IsKeyDown(value))
            {
                if (pressed)
                {
                    return false;
                }

                pressed = true;
                return true;
            }

            pressed = false;
            return false;
        }
    }
}
