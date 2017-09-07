using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PoEHUD.HUD.Menu;

namespace PoEHUD.HUD.Settings
{
    public class ListNode
    {
        [JsonIgnore]
        public Action<string> OnValueSelected = delegate { };

        [JsonIgnore]
        public ListButton SettingsListButton;

        private string value;

        public ListNode()
        {
        }

        public string Value
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
                    OnValueSelected(value);
                }
                catch
                {
                    DebugPlugin.DebugPlugin.LogMessage("Error in function that subscribed for: ListNode.OnValueSelected", 10, SharpDX.Color.Red);
                }
            }
        }

        public static implicit operator string(ListNode node)
        {
            return node.Value;
        }

        public void SetListValues(List<string> values)
        {
            SettingsListButton.SetValues(values);
        }
    }
}
