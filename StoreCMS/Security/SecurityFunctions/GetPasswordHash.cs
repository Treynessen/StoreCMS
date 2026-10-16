namespace Treynessen.Security
{
    using System;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Options;
    using Treynessen.Database.Entities;

    public static partial class SecurityFunctions
    {
        // [Комментарий ИИ-агента]: Identity V3 в ASP.NET Core 2.2: PBKDF2-HMAC-SHA256 и случайная соль размером 128 бит.
        // [Комментарий ИИ-агента]: Индивидуальная legacy-соль сайта не читается и не требуется.
        private static readonly PasswordHasher<User> passwordHasher =
            new PasswordHasher<User>(
                Options.Create(new PasswordHasherOptions
                {
                    CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
                    IterationCount = 600000
                }));

        public static string GetPasswordHash(string password)
        {
            return passwordHasher.HashPassword(new User(), password);
        }

        public static bool VerifyPassword(User user, string password)
        {
            if (user == null || string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(password))
                return false;
            try
            {
                // [Комментарий ИИ-агента]: Поддержка MD5 и Identity V2 намеренно исключена.
                var payload = Convert.FromBase64String(user.Password);
                if (payload.Length != 61 || payload[0] != 1
                    || payload[9] != 0 || payload[10] != 0 || payload[11] != 0 || payload[12] != 16)
                    return false;
                return passwordHasher.VerifyHashedPassword(user, user.Password, password)
                    != PasswordVerificationResult.Failed;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
