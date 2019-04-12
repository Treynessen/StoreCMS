using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Context;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static bool HasMainPage(CMSDatabase db)
        {
            return db.UsualPages.FirstOrDefaultAsync
                (up => up.RequestPathWithoutAlias.Equals("/") 
                && up.Alias.Equals("index")).Result == null ? false : true;
        }
    }
}
