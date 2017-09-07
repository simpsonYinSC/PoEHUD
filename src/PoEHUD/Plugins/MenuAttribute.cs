using System;

namespace PoEHUD.Plugins
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MenuAttribute : Attribute
    {
        public string MenuName;
        public string Tooltip;
        public int Index = -1;
        public int ParentIndex = -1;

        public MenuAttribute(string menuName)
        {
            MenuName = menuName;
        }

        public MenuAttribute(string menuName, string tooltip) : this(menuName)
        {
            Tooltip = tooltip;
        }

        public MenuAttribute(string menuName, int index)
        {
            MenuName = menuName;
            Index = index;
        }

        public MenuAttribute(string menuName, string tooltip, int index) : this(menuName, index)
        {
            Tooltip = tooltip;
        }

        public MenuAttribute(string menuName, int index, int parentIndex)
        {
            MenuName = menuName;
            Index = index;
            ParentIndex = parentIndex;
        }

        public MenuAttribute(string menuName, string tooltip, int index, int parentIndex) : this(menuName, index, parentIndex)
        {
            Tooltip = tooltip;
        }
    }
}
