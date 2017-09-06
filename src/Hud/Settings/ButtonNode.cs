using System;
using Newtonsoft.Json;

namespace PoEHUD.HUD.Settings
{
    public class ButtonNode
    {
        [JsonIgnore]
        public Action OnPressed = delegate { };

        public ButtonNode()
        {
        }
    }
}
