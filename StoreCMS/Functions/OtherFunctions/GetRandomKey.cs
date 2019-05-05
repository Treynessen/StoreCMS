using System;
using System.Text;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static string GetRandomKey(int minLength, int maxLength)
        {
            Random rand = new Random();
            string symbols = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890?!@#$%&_~-+=";
            int length = rand.Next(minLength, maxLength + 1);
            StringBuilder builder = new StringBuilder(length);
            for (int i = 0; i < length; ++i)
                builder.Append(symbols[rand.Next(0, symbols.Length)]);
            return builder.ToString();
        }
    }
}