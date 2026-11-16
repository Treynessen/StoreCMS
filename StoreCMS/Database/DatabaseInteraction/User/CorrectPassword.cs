using System;
using Treynessen.Security.Installation.Bootstrap;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static bool CorrectPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 5 || password.Equals(BootstrapPlan.Login, StringComparison.OrdinalIgnoreCase))
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
