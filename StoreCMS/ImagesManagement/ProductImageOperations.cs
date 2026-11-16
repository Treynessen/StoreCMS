using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.AspNetCore.DataProtection;
using Treynessen.Database.Entities;

namespace Treynessen.ImagesManagement
{
    public static partial class ProductImageOperations
    {
        public static string[] GetOriginalNames(string folder, string alias)
        {
            if (string.IsNullOrEmpty(alias) || alias.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
                || alias.Contains('/') || alias.Contains('\\') || !Directory.Exists(folder)) return new string[0];
            var regex = new Regex("\\A" + Regex.Escape(alias) + "(?:_([2-9]|[1-9][0-9]+))?\\.jpg\\z");
            return Directory.GetFiles(folder).Select(Path.GetFileName).Where(n => regex.IsMatch(n))
                .OrderBy(n => n == alias + ".jpg" ? 0 : n.Length).ThenBy(n => n, StringComparer.Ordinal).ToArray();
        }

        // [Комментарий ИИ-агента]: Снимок связывает набор с байтами и временными атрибутами, токен — с конкретной строкой и товаром.
        private static string Snapshot(string folder, string alias)
        {
            var value = new StringBuilder(GetVersion(folder));
            foreach (string name in GetOriginalNames(folder, alias))
            {
                string path = Path.Combine(folder, name);
                using (var stream = File.OpenRead(path))
                using (var sha = SHA256.Create())
                    value.Append(name).Append(':').Append(File.GetCreationTimeUtc(path).Ticks).Append(':')
                        .Append(File.GetLastWriteTimeUtc(path).Ticks).Append(':').Append(Convert.ToBase64String(sha.ComputeHash(stream))).Append('\n');
            }
            return Hash(Encoding.UTF8.GetBytes(value.ToString()));
        }

        public static string CreateToken(ProductPage product, string folder, string name, IDataProtectionProvider provider)
        {
            using (Acquire(folder))
                return provider.CreateProtector("StoreCMS.ProductImage.Delete.v1")
                    .Protect(product.ID + "\n" + name + "\n" + Snapshot(folder, product.Alias));
        }

        public static Dictionary<string, string> CreateTokens(ProductPage product, string folder, string[] urls, IDataProtectionProvider provider)
        {
            using (Acquire(folder))
            {
                string snapshot = Snapshot(folder, product.Alias);
                var protector = provider.CreateProtector("StoreCMS.ProductImage.Delete.v1");
                return urls.ToDictionary(url => url, url => protector.Protect(product.ID + "\n" + Path.GetFileName(url) + "\n" + snapshot));
            }
        }

        public static bool MatchesToken(ProductPage product, string folder, string name, string token, IDataProtectionProvider provider)
        {
            if (string.IsNullOrEmpty(token) || !GetOriginalNames(folder, product.Alias).Contains(name, StringComparer.Ordinal)) return false;
            try
            {
                return provider.CreateProtector("StoreCMS.ProductImage.Delete.v1").Unprotect(token)
                    == product.ID + "\n" + name + "\n" + Snapshot(folder, product.Alias);
            }
            catch (CryptographicException) { return false; }
        }
    }
}
