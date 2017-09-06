using System.Collections.Generic;

namespace PoEHUD.PoE.Elements
{
    public class HPBarElement : Element
    {
        public Entity MonsterEntity => ReadObject<Entity>(Address + 0x96C);
        public new List<HPBarElement> Children => GetChildren<HPBarElement>();
    }
}
