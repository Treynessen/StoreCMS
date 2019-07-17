using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Security;
using Treynessen.Database;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;
using Treynessen.FileManagerManagement;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [HttpPost]
        public IActionResult AdminPanel(Model model, LoginFormModel loginFormModel)
        {
            if (model.PageId == AdminPanelPages.LoginForm)
            {
                if (SecurityFunctions.IsValidLoginFormData(db, loginFormModel, HttpContext))
                    return StatusCode(200);
                else return StatusCode(401);
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
                    model.PageModel.PageType = PagesManagement.PageType.Usual;
                    DatabaseInteraction.AddPage(db, model.PageModel, HttpContext, out bool pageAdded);
                    if (pageAdded)
                    {
                        string createdPageUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditPage}&itemID={model.PageModel.ID}";
                        HttpContext.Response.Headers.Add("location", createdPageUrl);
                        return StatusCode(201);
                    }
                    else return StatusCode(422);

                case AdminPanelPages.EditPage:
                    model.PageType = PagesManagement.PageType.Usual;
                    DatabaseInteraction.EditPage(db, model, HttpContext, out bool pageEdited);
                    if (pageEdited) return StatusCode(200);
                    else return StatusCode(422);

                case AdminPanelPages.AddCategory:
                    model.PageModel.PageType = PagesManagement.PageType.Category;
                    DatabaseInteraction.AddPage(db, model.PageModel, HttpContext, out bool categoryAdded);
                    if (categoryAdded)
                    {
                        string createdPageUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditCategory}&itemID={model.PageModel.ID}";
                        HttpContext.Response.Headers.Add("location", createdPageUrl);
                        return StatusCode(201);
                    }
                    else return StatusCode(422);

                case AdminPanelPages.EditCategory:
                    model.PageType = PagesManagement.PageType.Category;
                    DatabaseInteraction.EditPage(db, model, HttpContext, out bool categoryEdited);
                    if (categoryEdited) return StatusCode(200);
                    else return StatusCode(422);

                case AdminPanelPages.AddProduct:
                    DatabaseInteraction.AddProduct(db, model.PageModel, model.itemID, HttpContext, out bool productAdded);
                    if (productAdded)
                    {
                        string createdPageUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditProduct}&itemID={model.PageModel.ID}";
                        HttpContext.Response.Headers.Add("location", createdPageUrl);
                        return StatusCode(201);
                    }
                    else return StatusCode(422);

                case AdminPanelPages.EditProduct:
                    DatabaseInteraction.EditProduct(db, model.PageModel, model.itemID, HttpContext, out bool productEdited);
                    if (productEdited) return StatusCode(200);
                    else return StatusCode(422);

                case AdminPanelPages.AddTemplate:
                    DatabaseInteraction.AddTemplate(db, model.TemplateModel, HttpContext, out bool templateAdded);
                    if (templateAdded)
                    {
                        string createdTemplateUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditTemplate}&itemID={model.TemplateModel.ID}";
                        HttpContext.Response.Headers.Add("location", createdTemplateUrl);
                        return StatusCode(201);
                    }
                    else return StatusCode(422);

                case AdminPanelPages.EditTemplate:
                    DatabaseInteraction.EditTemplate(db, model.itemID, model.TemplateModel, HttpContext, out bool templateEdited);
                    if (templateEdited) return StatusCode(200);
                    else return StatusCode(422);

                case AdminPanelPages.AddChunk:
                    DatabaseInteraction.AddChunk(db, model.TemplateModel, HttpContext, out bool chunkAdded);
                    if (chunkAdded)
                    {
                        string createdTemplateUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditChunk}&itemID={model.TemplateModel.ID}";
                        HttpContext.Response.Headers.Add("location", createdTemplateUrl);
                        return StatusCode(201);
                    }
                    else return StatusCode(422);

                case AdminPanelPages.EditChunk:
                    DatabaseInteraction.EditChunk(db, model.itemID, model.TemplateModel, HttpContext, out bool chunkEdited);
                    if (chunkEdited) return StatusCode(200);
                    else return StatusCode(422);

                case AdminPanelPages.CreateFolder:
                    FileManagerManagementFunctions.CreateFolder(model.Path, model.Name, HttpContext);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.FileManager}&path={model.Path}");

                case AdminPanelPages.CreateStyle:
                    FileManagerManagementFunctions.CreateCssFile(model.Path, model.Name, HttpContext);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.FileManager}&path={model.Path}");

                case AdminPanelPages.CreateScript:
                    FileManagerManagementFunctions.CreateScriptFile(model.Path, model.Name, HttpContext);
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

                case AdminPanelPages.EditScript:
                    FileManagerManagementFunctions.EditScriptFile(model.Path, model.StyleModel, HttpContext, out bool scriptFileEdited);
                    if (!scriptFileEdited)
                    {
                        HttpContext.Items["pageID"] = AdminPanelPages.EditScript;
                        HttpContext.Items["IsIncorrect"] = true;
                        return View("FileManager/EditScriptFile", model.StyleModel);
                    }
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditScript}&path={model.Path.Substring(0, model.Path.LastIndexOf('>') + 1)}{model.StyleModel.FileName}.js");

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