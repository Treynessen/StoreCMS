using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Trane.Database.Context;
using Trane.Database.Entities;

namespace Trane.Functions
{
    public static partial class ActionsWithDatabase
    {
        public static bool AddSimplePage(CMSDatabase db, SimplePage page, HttpContext context)
        {
            if (string.IsNullOrEmpty(page.BreadcrumbName))
                page.BreadcrumbName = OtherFunctions.GetCorrectBreadcrumbName(page.Title, context);
            else
                page.BreadcrumbName = OtherFunctions.GetCorrectBreadcrumbName(page.BreadcrumbName, context);

            page.RequestPath = OtherFunctions.GetRequestPath(db, page);
            
            OtherFunctions.SetUniqueBreadcrumbName(db, page);

            if (!Validator.TryValidateObject(page, new ValidationContext(page), null))
                return false;

            page.ID = OtherFunctions.GetDatabaseRawID(db.SimplePages);
            db.Add(page);
            db.SaveChanges();
            return true;
        }
    }
}
