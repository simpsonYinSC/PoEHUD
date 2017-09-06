namespace PoEHUD.Models
{
    public class ItemClass
    {
        public ItemClass(string className, string classCategory)
        {
            ClassName = className;
            ClassCategory = classCategory;
        }

        public string ClassName { get; set; }
        public string ClassCategory { get; set; }
    }
}
