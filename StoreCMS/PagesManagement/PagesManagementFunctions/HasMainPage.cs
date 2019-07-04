using System;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.PagesManagement
{
    public static partial class PagesManagementFunctions
    {
        public static bool HasMainPage(CMSDatabase db)
        {
            UsualPage usualPage = db.UsualPages.FirstOrDefaultAsync(up => up.RequestPath.Equals("/", StringComparison.InvariantCulture)).Result;
            if (usualPage == null)
                return false;
            db.Entry(usualPage).State = EntityState.Detached;
            return true;
        }
    }
}