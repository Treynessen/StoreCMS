namespace Treynessen.PagesManagement
{
    public static partial class PagesManagementFunctions
    {
        public static int GetHashFromRequestPath(string requestPath)
        {
            if (requestPath.Length == 0)
                return 0;
            int constVal = int.MaxValue / requestPath.Length;
            int hashedValue = constVal;
            for (int i = 0; i < requestPath.Length; ++i)
            {
                hashedValue = unchecked(hashedValue + requestPath[i]);
                hashedValue = unchecked(hashedValue * constVal);
            }
            return hashedValue;
        }
    }
}