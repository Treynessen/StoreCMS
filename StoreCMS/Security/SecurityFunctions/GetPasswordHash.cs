namespace Treynessen.Security
{
    public static partial class SecurityFunctions
    {
        public static string GetPasswordHash(string password)
        {
            string salt = "salt";
            for (int i = 1; i <= 20; ++i)
            {
                password = GetMd5Hash(password + salt);
            }
            return password;
        }
    }
}