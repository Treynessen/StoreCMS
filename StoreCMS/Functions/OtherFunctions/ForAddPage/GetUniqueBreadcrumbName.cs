using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Trane.Database.Context;
using Trane.Database.Entities;

namespace Trane.Functions
{
    public static partial class OtherFunctions
    {
        public static void SetUniqueBreadcrumbName(CMSDatabase db, Page page)
        {
            int index = 0;
            bool has = false;
            do
            {
                string currentPath = $"{page.RequestPath}{(index == 0 ? "" : index.ToString())}";
                has = false;
                if (index == int.MaxValue)
                {
                    page.BreadcrumbName += index.ToString();
                    page.RequestPath += index.ToString();
                    index = 1;
                }
                var simplePage = db.SimplePages.FirstOrDefaultAsync(sp => sp.RequestPath.Equals(currentPath));
                var categoryPage = db.CategoryPages.FirstOrDefaultAsync(cp => cp.RequestPath.Equals(currentPath));
                var productPage = db.ProductPages.FirstOrDefaultAsync(pp => pp.RequestPath.Equals(currentPath));
                if (simplePage.Result != null) has = true;
                else if (categoryPage.Result != null) has = true;
                else if (productPage.Result != null) has = true;
                if (has && index == 0)
                {
                    page.BreadcrumbName += "_";
                    page.RequestPath += "_";
                }
                if (!has && index != 0)
                {
                    page.BreadcrumbName += index.ToString();
                    page.RequestPath += index.ToString();
                }
                ++index;
            } while (has);
        }
    }
}
