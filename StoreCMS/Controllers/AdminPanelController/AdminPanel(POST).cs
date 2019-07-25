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
                        string createdCategoryUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditCategory}&itemID={model.PageModel.ID}";
                        HttpContext.Response.Headers.Add("location", createdCategoryUrl);
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
                        string createdProductUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditProduct}&itemID={model.PageModel.ID}";
                        HttpContext.Response.Headers.Add("location", createdProductUrl);
                        return StatusCode(201);
                    }
                    else return StatusCode(422);

                case AdminPanelPages.EditProduct:
                    DatabaseInteraction.EditProduct(db, model.PageModel, model.itemID, HttpContext, out bool productEdited);
                    if (productEdited) return StatusCode(200);
                    else return StatusCode(422);

                case AdminPanelPages.AddRedirection:
                    DatabaseInteraction.AddRedirection(db, model.RedirectionModel, out bool redirectionAdded);
                    if (redirectionAdded) return StatusCode(201);
                    else return StatusCode(422);

                case AdminPanelPages.EditRedirection:
                    DatabaseInteraction.EditRedirection(db, model.itemID, model.RedirectionModel, out bool redirectionEdited);
                    if (redirectionEdited) return StatusCode(200);
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
                        string createdChunkUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditChunk}&itemID={model.TemplateModel.ID}";
                        HttpContext.Response.Headers.Add("location", createdChunkUrl);
                        return StatusCode(201);
                    }
                    else return StatusCode(422);

                case AdminPanelPages.EditChunk:
                    DatabaseInteraction.EditChunk(db, model.itemID, model.TemplateModel, HttpContext, out bool chunkEdited);
                    if (chunkEdited) return StatusCode(200);
                    else return StatusCode(422);

                case AdminPanelPages.CreateFolder:
                    FileManagerManagementFunctions.CreateFolder(model.Path, model.Name, HttpContext, out bool folderCreated);
                    if (folderCreated) return StatusCode(201);
                    else return StatusCode(422);

                case AdminPanelPages.CreateStyle:
                    FileManagerManagementFunctions.CreateCssFile(model.Path, model.Name, HttpContext, out bool styleFileCreated);
                    if (styleFileCreated) return StatusCode(201);
                    else return StatusCode(422);

                case AdminPanelPages.CreateScript:
                    FileManagerManagementFunctions.CreateScriptFile(model.Path, model.Name, HttpContext, out bool scriptFileCreated);
                    if (scriptFileCreated) return StatusCode(201);
                    else return StatusCode(422);

                case AdminPanelPages.EditStyle:
                    FileManagerManagementFunctions.EditCssFile(model.Path, model.StyleModel, HttpContext, out string editedStylePath, out bool cssFileEdited);
                    if (cssFileEdited)
                    {
                        string editedCssFileUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditStyle}&path={editedStylePath}";
                        HttpContext.Response.Headers.Add("location", editedCssFileUrl);
                        return StatusCode(200);
                    }
                    else return StatusCode(422);

                case AdminPanelPages.EditScript:
                    FileManagerManagementFunctions.EditScriptFile(model.Path, model.StyleModel, HttpContext, out string editedScriptPath, out bool scriptFileEdited);
                    if (scriptFileEdited)
                    {
                        string editedScriptFileUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditScript}&path={editedScriptPath}";
                        HttpContext.Response.Headers.Add("location", editedScriptFileUrl);
                        return StatusCode(200);
                    }
                    else return StatusCode(422);

                case AdminPanelPages.AddUserType:
                    DatabaseInteraction.AddUserType(db, model.UserTypeModel, out bool userTypeAdded);
                    if (userTypeAdded) return StatusCode(201);
                    else return StatusCode(422);

                case AdminPanelPages.EditUserType:
                    DatabaseInteraction.EditUserType(db, model.itemID, model.UserTypeModel, out bool userTypeEdited);
                    if (userTypeEdited) return StatusCode(200);
                    else return StatusCode(422);

                case AdminPanelPages.AddSynonymForString:
                    DatabaseInteraction.AddSynonymForString(db, model.SynonymForStringModel, out bool synonymForStringAdded);
                    if (synonymForStringAdded) return StatusCode(201);
                    else return StatusCode(422);

                case AdminPanelPages.EditSynonymForString:
                    DatabaseInteraction.EditSynonymForString(db, model.itemID, model.SynonymForStringModel, out bool synonymForStringEdited);
                    if (synonymForStringEdited) return StatusCode(200);
                    else return StatusCode(422);

                case AdminPanelPages.EditSettings:
                    return EditSettings(model.SettingsModel, HttpContext);

                default:
                    return RedirectToAction(nameof(AdminPanel));
            }
        }
    }
}