using System;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static bool HasMainPage(CMSDatabase db)
        {
            UsualPage usualPage = db.UsualPages.FirstOrDefaultAsync(up => up.RequestPathWithoutAlias.Equals("/") && up.Alias.Equals("index", StringComparison.InvariantCulture)).Result;
            if (usualPage == null)
                return false;
            db.Entry(usualPage).State = EntityState.Detached;
            return true;
        }
    }
}
