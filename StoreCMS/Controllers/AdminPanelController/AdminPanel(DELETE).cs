using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Security;
using Treynessen.Database;
using Treynessen.PagesManagement;
using Treynessen.AdminPanelTypes;
using Treynessen.ImagesManagement;
using Treynessen.Database.Entities;
using Treynessen.FileManagerManagement;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [HttpDelete]
        public IActionResult AdminPanel(AdminPanelPages pageID, int? itemID, int? imageID, string path)
        {
            AccessLevelConfiguration accessLevelConfiguration = HttpContext.RequestServices.GetService<AccessLevelConfiguration>();
            HttpContext.Items["AccessLevelConfiguration"] = accessLevelConfiguration;
            User user = SecurityFunctions.CheckCookies(db, HttpContext);
            if (!SecurityFunctions.HasAccessTo(pageID, user, HttpContext))
                return RedirectToAction(nameof(AdminPanel));

            switch (pageID)
            {
                case AdminPanelPages.DeletePage:
                    DatabaseInteraction.DeletePage(db, PageType.Usual, itemID, HttpContext, out bool pageDeleted);
                    if (pageDeleted) return StatusCode(200);
                    else return StatusCode(404);

                case AdminPanelPages.DeleteCategory:
                    DatabaseInteraction.DeletePage(db, PageType.Category, itemID, HttpContext, out bool categoryDeleted);
                    if (categoryDeleted) return StatusCode(200);
                    else return StatusCode(404);

                case AdminPanelPages.DeleteProduct:
                    DatabaseInteraction.DeleteProduct(db, itemID, HttpContext, out bool productDeleted);
                    if (productDeleted) return StatusCode(200);
                    else return StatusCode(404);

                case AdminPanelPages.DeleteProductImage:
                    ImagesManagementFunctions.DeleteProductImage(db, itemID, imageID, HttpContext, out bool productImageDeleted);
                    if (productImageDeleted) return StatusCode(200);
                    else return StatusCode(404);

                case AdminPanelPages.DeleteTemplate:
                    DatabaseInteraction.DeleteTemplate(db, itemID, HttpContext, out bool templateDeleted);
                    if (templateDeleted) return StatusCode(200);
                    else return StatusCode(404);

                case AdminPanelPages.DeleteChunk:
                    DatabaseInteraction.DeleteChunk(db, itemID, HttpContext, out bool chunkDeleted);
                    if(chunkDeleted) return StatusCode(200);
                    else return StatusCode(404);

                case AdminPanelPages.DeleteFileOrFolder:
                    FileManagerManagementFunctions.DeleteFileOrFolder(path, HttpContext, out string redirectPath);
                    if (redirectPath == null)
                        return StatusCode(404);
                    else
                    {
                        string redirectUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.FileManager}&path={redirectPath}";
                        HttpContext.Response.Headers.Add("location", redirectUrl);
                        return StatusCode(200);
                    }

                default:
                    return RedirectToAction(nameof(AdminPanel));
            }
        }
    }
}