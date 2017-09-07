namespace PoEHUD.Framework
{
    public class IntRange
    {
        public IntRange()
        {
            Min = int.MaxValue;
            Max = int.MinValue;
        }

        public IntRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public int Min { get; private set; }
        public int Max { get; private set; }

        public void Include(int value)
        {
            if (value < Min)
            {
                Min = value;
            }

            if (value > Max)
            {
                Max = value;
            }
        }

        public override string ToString()
        {
            return string.Concat(Min, " - ", Max);
        }

        internal float GetPercentage(int val)
        {
            if (Min == Max)
            {
                return 1;
            }

            return (float)(val - Min) / (Max - Min);
        }

        internal bool HasSpread()
        {
            return Max != Min;
        }
    }
}
