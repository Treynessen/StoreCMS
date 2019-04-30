using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.Functions;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [HttpPost]
        public IActionResult AdminPanel(AdminPanelModel model, LoginFormModel lfModel)
        {
            User user = DataCheck.CheckCookies(db, HttpContext);
            if (user == null)
            {
                if (!DataCheck.IsValidLoginFormData(db, lfModel, HttpContext))
                    return LoginForm(lfModel);
                return RedirectToAction(nameof(AdminPanel));
            }
            if (!DataCheck.HasAccessTo(model.PageId.Value, user, HttpContext))
                return RedirectToAction(nameof(AdminPanel));

            HttpContext.Items["User"] = user;

            switch (model.PageId)
            {
                case AdminPanelPages.AddPage:
                    if (ActionsWithDatabase.AddPage(db, model.PageModel, HttpContext) == false)
                        return AddPage(model.PageModel);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Pages}");

                case AdminPanelPages.EditPage:
                    if (ActionsWithDatabase.EditPage(db, model, HttpContext) == false)
                        return EditPage(model.PageType, model.itemID, model.PageModel);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditPage}&pageType={(int)model.PageType}&itemID={model.itemID}");

                case AdminPanelPages.DeletePage:
                    ActionsWithDatabase.DeletePage(db, model.PageType, model.itemID);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Pages}");

                case AdminPanelPages.AddProduct:
                    model.PageModel.PageType = PageType.Product;
                    model.PageModel.PreviousPageID = model.itemID;
                    if (ActionsWithDatabase.AddPage(db, model.PageModel, HttpContext) == false)
                        return AddProduct(model.PageModel);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.ShowCategoryProducts}&itemID={model.itemID}");

                case AdminPanelPages.AddProductImage:
                    OtherFunctions.AddProductImageToServer(db, model.uploadedFile, model.itemID, HttpContext);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.ProductImages}&itemID={model.itemID}");

                case AdminPanelPages.EditProduct:
                    model.PageType = PageType.Product;
                    if (ActionsWithDatabase.EditPage(db, model, HttpContext) == false)
                        return EditProduct(model.itemID, model.PageModel);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditProduct}&itemID={model.itemID}");

                case AdminPanelPages.DeleteProduct:
                    if (model.itemID.HasValue)
                    {
                        ProductPage page = db.ProductPages.FirstOrDefaultAsync(p => p.ID == model.itemID).Result;
                        ActionsWithDatabase.DeletePage(db, page);
                        return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.ShowCategoryProducts}&itemID={page.PreviousPageID}");
                    }
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");

                case AdminPanelPages.DeleteProductImage:
                    OtherFunctions.DeleteProductImage(db, model.itemID, model.imageID, HttpContext);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.ProductImages}&itemID={model.itemID}");

                case AdminPanelPages.AddTemplate:
                    if (ActionsWithDatabase.AddTemplate(db, model.TemplateModel, HttpContext) == false)
                        return AddTemplate(model.TemplateModel);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Templates}");

                case AdminPanelPages.EditTemplate:
                    if (ActionsWithDatabase.EditTemplate(db, model, HttpContext) == false)
                        return EditTemplate(model.itemID, model.TemplateModel);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditTemplate}&itemID={model.itemID}");

                case AdminPanelPages.DeleteTemplate:
                    ActionsWithDatabase.DeleteTemplate(db, model.itemID, HttpContext);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Templates}");

                case AdminPanelPages.AddTemplateChunk:
                    if (ActionsWithDatabase.AddTemplateChunk(db, model.TemplateModel, HttpContext) == false)
                        return AddTemplateChunk(model.TemplateModel);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.TemplateChunks}");

                case AdminPanelPages.EditTemplateChunk:
                    if (ActionsWithDatabase.EditTemplateChunk(db, model, HttpContext) == false)
                        return EditTemplateChunk(model.itemID, model.TemplateModel);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditTemplateChunk}&itemID={model.itemID}");

                case AdminPanelPages.DeleteTemplateChunk:
                    ActionsWithDatabase.DeleteTemplateChunk(db, model.itemID, HttpContext);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.TemplateChunks}");

                default:
                    return RedirectToAction(nameof(AdminPanel));
            }
        }
    }
}
