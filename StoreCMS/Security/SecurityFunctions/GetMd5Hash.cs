using System.Text;
using System.Security.Cryptography;

namespace Treynessen.Security
{
    public static partial class SecurityFunctions
    {
        public static string GetMd5Hash(string value)
        {
            string md5Hash = null;
            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
                StringBuilder hashBuilder = new StringBuilder(hashBytes.Length * 2);
                for (int i = 0; i < hashBytes.Length; ++i)
                    hashBuilder.AppendFormat("{0:x2}", hashBytes[i]);
                md5Hash = hashBuilder.ToString();
            }
            return md5Hash;
        }
    }
}