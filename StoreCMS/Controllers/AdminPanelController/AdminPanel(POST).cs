using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Security;
using Treynessen.Database;
using Treynessen.AdminPanelTypes;
using Treynessen.ImagesManagement;
using Treynessen.Database.Entities;
using Treynessen.FileManagerManagement;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [HttpPost]
        public IActionResult AdminPanel(Model model, LoginFormModel loginFormModel)
        {
            if (loginFormModel.HasData)
            {
                if (SecurityFunctions.IsValidLoginFormData(db, loginFormModel, HttpContext))
                    return RedirectToAction(nameof(AdminPanel));
                return LoginForm(loginFormModel);
            }
            AccessLevelConfiguration accessLevelConfiguration = HttpContext.RequestServices.GetService<AccessLevelConfiguration>();
            HttpContext.Items["AccessLevelConfiguration"] = accessLevelConfiguration;
            User user = SecurityFunctions.CheckCookies(db, HttpContext);
            if (!SecurityFunctions.HasAccessTo(model.PageId, user, HttpContext))
                return RedirectToAction(nameof(AdminPanel));

            HttpContext.Items["User"] = user;

            switch (model.PageId)
            {
                case AdminPanelPages.AddPage:
                    DatabaseInteraction.AddPage(db, model.PageModel, HttpContext, out bool pageAdded);
                    if (pageAdded)
                        return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Pages}");
                    else return AddPage(model.PageModel);

                case AdminPanelPages.EditPage:
                    DatabaseInteraction.EditPage(db, model, HttpContext, out bool pageEdited);
                    if (pageEdited)
                        return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditPage}&pageType={(int)model.PageType}&itemID={model.itemID}");
                    return EditPage(model.PageType, model.itemID, model.PageModel);

                case AdminPanelPages.DeletePage:
                    DatabaseInteraction.DeletePage(db, model.PageType, model.itemID, HttpContext);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Pages}");

                case AdminPanelPages.AddProduct:
                    DatabaseInteraction.AddProduct(db, model.PageModel, model.itemID, HttpContext, out bool productAdded);
                    if (productAdded)
                        return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.CategoryProducts}&itemID={model.itemID}");
                    else return AddProduct(model.PageModel);

                case AdminPanelPages.EditProduct:
                    DatabaseInteraction.EditProduct(db, model.PageModel, model.itemID, HttpContext, out bool productEdited);
                    if (productEdited)
                        return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditProduct}&itemID={model.itemID}");
                    else return EditProduct(model.itemID, model.PageModel);

                case AdminPanelPages.DeleteProduct:
                    DatabaseInteraction.DeleteProduct(db, model.itemID, HttpContext, out int? categoryID);
                    if (categoryID.HasValue)
                        return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.CategoryProducts}&itemID={categoryID.Value}");
                    else return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Categories}");

                case AdminPanelPages.AddProductImage:
                    ImagesManagementFunctions.AddProductImageToServer(db, model.uploadedFile, model.itemID, HttpContext);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.ProductImages}&itemID={model.itemID}");

                case AdminPanelPages.DeleteProductImage:
                    ImagesManagementFunctions.DeleteProductImage(db, model.itemID, model.imageID, HttpContext);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.ProductImages}&itemID={model.itemID}");

                case AdminPanelPages.AddTemplate:
                    DatabaseInteraction.AddTemplate(db, model.TemplateModel, HttpContext, out bool templateAdded);
                    if (templateAdded)
                        return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Templates}");
                    else return AddTemplate(model.TemplateModel);

                case AdminPanelPages.EditTemplate:
                    DatabaseInteraction.EditTemplate(db, model.itemID, model.TemplateModel, HttpContext, out bool templateEdited);
                    if (templateEdited)
                        return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditTemplate}&itemID={model.itemID}");
                    else return EditTemplate(model.itemID, model.TemplateModel);

                case AdminPanelPages.DeleteTemplate:
                    DatabaseInteraction.DeleteTemplate(db, model.itemID, HttpContext);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Templates}");

                case AdminPanelPages.AddChunk:
                    DatabaseInteraction.AddChunk(db, model.TemplateModel, HttpContext, out bool chunkAdded);
                    if (chunkAdded)
                        return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Chunks}");
                    else return AddChunk(model.TemplateModel);

                case AdminPanelPages.EditChunk:
                    DatabaseInteraction.EditChunk(db, model.itemID, model.TemplateModel, HttpContext, out bool chunkEdited);
                    if (chunkEdited)
                        return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditChunk}&itemID={model.itemID}");
                    else return EditChunk(model.itemID, model.TemplateModel);

                case AdminPanelPages.DeleteChunk:
                    DatabaseInteraction.DeleteChunk(db, model.itemID, HttpContext);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Chunks}");

                case AdminPanelPages.UploadFile:
                    FileManagerManagementFunctions.UploadFileToServer(model.Path, model.uploadedFile, HttpContext);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.FileManager}&path={model.Path}");

                case AdminPanelPages.CreateFolder:
                    FileManagerManagementFunctions.CreateFolder(model.Path, model.Name, HttpContext);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.FileManager}&path={model.Path}");

                case AdminPanelPages.CreateStyle:
                    FileManagerManagementFunctions.CreateCssFile(model.Path, model.Name, HttpContext);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.FileManager}&path={model.Path}");

                case AdminPanelPages.EditStyle:
                    FileManagerManagementFunctions.EditCssFile(model.Path, model.StyleModel, HttpContext, out bool cssFileEdited);
                    if (!cssFileEdited)
                    {
                        HttpContext.Items["pageID"] = AdminPanelPages.EditStyle;
                        HttpContext.Items["IsIncorrect"] = true;
                        return View("FileManager/EditCssFile", model.StyleModel);
                    }
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditStyle}&path={model.Path.Substring(0, model.Path.LastIndexOf('>') + 1)}{model.StyleModel.FileName}.css");

                case AdminPanelPages.DeleteFileOrFolder:
                    FileManagerManagementFunctions.DeleteFileOrFolder(model.Path, HttpContext, out string redirectPath);
                    if (string.IsNullOrEmpty(redirectPath))
                        return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.FileManager}");
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.FileManager}&path={redirectPath}");

                case AdminPanelPages.EditSettings:
                    return EditSettings(model.SettingsModel, HttpContext);

                default:
                    return RedirectToAction(nameof(AdminPanel));
            }
        }
    }
}