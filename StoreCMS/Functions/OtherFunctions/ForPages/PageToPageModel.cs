using System;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static PageModel PageToPageModel(Page page)
        {
            if (page == null)
                return null;
            PageModel model = new PageModel();
            switch (page)
            {
                case UsualPage up:
                    if (!up.PreviousPageID.HasValue && page.Alias.Equals("index", StringComparison.InvariantCulture))
                        model.IsMainPage = true;
                    model.PageType = PageType.Usual;
                    model.PreviousPageID = up.PreviousPageID;
                    break;
                case CategoryPage cp:
                    model.PageType = PageType.Category;
                    model.PreviousPageID = cp.PreviousPageID;
                    break;
                case ProductPage pp:
                    model.PageType = PageType.Product;
                    model.Price = pp.Price;
                    model.OldPrice = pp.OldPrice;
                    model.ShortDescription = pp.ShortDescription;
                    model.PreviousPageID = pp.PreviousPageID;
                    break;
                default:
                    return null;
            }
            model.Title = page.Title;
            model.BreadcrumbName = page.BreadcrumbName;
            model.Alias = page.Alias;
            model.TemplateId = page.TemplateId;
            model.Content = page.Content;
            model.Published = page.Published;
            model.PageDescription = page.PageDescription;
            model.PageKeywords = page.PageKeywords;
            model.IsRobotIndex = page.IsRobotIndex;
            return model;
        }
    }
}
