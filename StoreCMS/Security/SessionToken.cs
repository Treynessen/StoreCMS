using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace Treynessen.Security
{
    public static class SessionToken
    {
        public static string Create()
        {
            var bytes = new byte[32];
            using (var random = RandomNumberGenerator.Create()) random.GetBytes(bytes);
            return WebEncoders.Base64UrlEncode(bytes);
        }

        public static bool TryHash(string token, out string hash)
        {
            hash = null;
            if (token == null || token.Length != 43) return false;
            foreach (char c in token)
                if (!(c >= 'A' && c <= 'Z') && !(c >= 'a' && c <= 'z')
                    && !(c >= '0' && c <= '9') && c != '-' && c != '_') return false;
            byte[] bytes;
            try { bytes = WebEncoders.Base64UrlDecode(token); }
            catch (FormatException) { return false; }
            if (bytes.Length != 32 || WebEncoders.Base64UrlEncode(bytes) != token) return false;
            using (var sha = SHA256.Create())
                hash = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
            return true;
        }
    }
}
