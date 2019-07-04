using System;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;

namespace Treynessen.PagesManagement
{
    public static partial class PagesManagementFunctions
    {
        public static PageModel PageToPageModel(Page page)
        {
            if (page == null)
                return null;
            PageModel model = new PageModel();
            switch (page)
            {
                case UsualPage up:
                    model.PageType = PageType.Usual;
                    if (up.RequestPath.Equals("/", StringComparison.InvariantCulture))
                        model.IsMainPage = true;
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
                    model.SpecialProduct = pp.SpecialProduct;
                    model.PreviousPageID = pp.PreviousPageID;
                    break;
                default:
                    return null;
            }
            model.Title = page.Title;
            model.PageName = page.PageName;
            model.Alias = page.Alias;
            model.TemplateId = page.TemplateId;
            model.Content = page.Content;
            model.Published = page.Published;
            model.PageDescription = page.PageDescription;
            model.PageKeywords = page.PageKeywords;
            model.IsIndex = page.IsIndex;
            model.IsFollow = page.IsFollow;
            return model;
        }
    }
}