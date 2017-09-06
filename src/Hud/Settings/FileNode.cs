using System;

namespace PoEHUD.HUD.Settings
{
    public sealed class FileNode
    {
        public Action OnFileChanged;

        private string value;

        public FileNode()
        {
        }

        public FileNode(string value)
        {
            Value = value;
        }

        public string Value
        {
            get => value;
            set
            {
                this.value = value;
                OnFileChanged?.Invoke();
            }
        }

        public static implicit operator string(FileNode node)
        {
            return node.Value;
        }

        public static implicit operator FileNode(string value)
        {
            return new FileNode(value);
        }
    }
}
