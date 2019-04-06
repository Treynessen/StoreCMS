using System.Collections.Generic;
using Trane.Database.Context;
using Trane.Database.Entities;

namespace Trane.Functions
{
    public static partial class OtherFunctions
    {
        public static LinkedList<string> GetBreadcrumbs(CMSDatabase db, Page page, LinkedList<string> breadcrumbs = null)
        {
            breadcrumbs = breadcrumbs == null ? new LinkedList<string>() : breadcrumbs;
            switch (page)
            {
                case SimplePage simple:
                    db.Entry(simple).Reference(p => p.PreviousPage).Load();
                    if (simple.PreviousPage != null)
                        GetBreadcrumbs(db, simple.PreviousPage, breadcrumbs);
                    break;
                case CategoryPage category:
                    db.Entry(category).Reference(p => p.PreviousPage).Load();
                    if (category.PreviousPage != null)
                        GetBreadcrumbs(db, category.PreviousPage, breadcrumbs);
                    break;
                case ProductPage product:
                    db.Entry(product).Reference(p => p.PreviousPage).Load();
                    if (product.PreviousPage != null)
                        GetBreadcrumbs(db, product.PreviousPage, breadcrumbs);
                    break;
            }
            breadcrumbs.AddLast(page.BreadcrumbName);
            return breadcrumbs;
        }
    }
}
