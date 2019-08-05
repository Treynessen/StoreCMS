using System;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        private static bool CorrectPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 5)
                return false;
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