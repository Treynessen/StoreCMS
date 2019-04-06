using System.Text;
using System.Collections.Generic;

namespace Trane.Functions
{
    public static partial class OtherFunctions
    {
        public static string BreadcrumbListToPath(LinkedList<string> breadcrumbs)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var b in breadcrumbs)
                builder.Append($"/{b}");
            return builder.ToString();
        }
    }
}
