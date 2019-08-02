using System;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        private static bool CorrectPassword(string password)
        {
            string availableSymbols = "qwertyuiopasdfghjklzxcvbnm1234567890-_!@#$%&?*№~=";
            foreach (var symbol in password)
            {
                if (!availableSymbols.Contains(symbol, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }
    }
}