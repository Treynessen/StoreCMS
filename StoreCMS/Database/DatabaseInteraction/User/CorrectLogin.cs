﻿using System;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        private static bool CorrectLogin(string login)
        {
            if (string.IsNullOrEmpty(login))
                return false;
            string availableSymbols = "qwertyuiopasdfghjklzxcvbnm1234567890-_";
            foreach (var symbol in login)
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