using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class ActionsWithDatabase
    {
        public static bool AddPage(CMSDatabase db, PageModel model, HttpContext context)
        {
            Page page = OtherFunctions.PageModelToPage(db, model, context);
            if (page != null)
            {
                switch (page)
                {
                    case UsualPage up:
                        up.ID = OtherFunctions.GetDatabaseRawID(db.UsualPages);
                        break;
                    case CategoryPage cp:
                        cp.ID = OtherFunctions.GetDatabaseRawID(db.CategoryPages);
                        break;
                    case ProductPage pp:
                        pp.ID = OtherFunctions.GetDatabaseRawID(db.ProductPages);
                        break;
                }
            }
            else
                return false;
            if (!Validator.TryValidateObject(page, new ValidationContext(page), null))
                return false;
            OtherFunctions.SetUniqueAliasName(db, page);
            db.Add(page);
            db.SaveChanges();
            return true;
        }
    }
}