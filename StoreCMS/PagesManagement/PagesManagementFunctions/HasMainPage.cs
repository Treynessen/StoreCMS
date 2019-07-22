using System;
using System.Linq;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.PagesManagement
{
    public static partial class PagesManagementFunctions
    {
        public static bool HasMainPage(CMSDatabase db)
        {
            UsualPage usualPage = db.UsualPages.FirstOrDefault(up => up.RequestPath.Equals("/", StringComparison.InvariantCulture));
            if (usualPage == null)
                return false;
            return true;
        }
    }
}