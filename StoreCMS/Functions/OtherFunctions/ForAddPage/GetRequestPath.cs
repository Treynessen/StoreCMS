using System.Collections.Generic;
using Trane.Database.Context;
using Trane.Database.Entities;

namespace Trane.Functions
{
    public static partial class OtherFunctions
    {
        public static string GetRequestPath(CMSDatabase db, Page page)
        {
            LinkedList<string> breadcrumbs = null;
            switch (page)
            {
                case SimplePage simple:
                    if (simple.PreviousPage != null)
                        breadcrumbs = GetBreadcrumbs(db, simple.PreviousPage);
                    break;
                case CategoryPage category:
                    if (category.PreviousPage != null)
                        breadcrumbs = GetBreadcrumbs(db, category.PreviousPage);
                    break;
                case ProductPage product:
                    if (product.PreviousPage != null)
                        breadcrumbs = GetBreadcrumbs(db, product.PreviousPage);
                    break;
            }
            if (breadcrumbs == null)
                return $"/{page.BreadcrumbName}";
            return BreadcrumbListToPath(breadcrumbs);
        }
    }
}
