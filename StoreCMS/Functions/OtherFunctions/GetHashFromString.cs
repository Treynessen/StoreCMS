namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static int GetHashFromString(string value)
        {
            if (value.Length == 0)
                return 0;
            int constVal = int.MaxValue / value.Length;
            int hashedValue = constVal;
            for (int i = 0; i < value.Length; ++i)
            {
                hashedValue = unchecked(hashedValue + value[i]);
                hashedValue = unchecked(hashedValue * constVal);
            }
            return hashedValue;
        }
    }
}