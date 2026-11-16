using System;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static string GetNameWithUnderscore(string name)
        {
            try
            {
                int lastUnderscore = name.LastIndexOf('_');
                if (lastUnderscore < name.Length)
                {
                    if (lastUnderscore == -1)
                        throw new FormatException();
                    Convert.ToInt32(name.Substring(lastUnderscore + 1));
                    name = name.Substring(0, lastUnderscore + 1);
                }
            }
            catch (FormatException)
            {
                name += "_";
            }
            return name;
        }
    }
}