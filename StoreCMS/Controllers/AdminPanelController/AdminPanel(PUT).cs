using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
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
        [HttpPut]
        public IActionResult AdminPanel(AdminPanelPages pageID, int? itemID, string path, IFormFile uploadedFile)
        {
            AccessLevelConfiguration accessLevelConfiguration = HttpContext.RequestServices.GetService<AccessLevelConfiguration>();
            HttpContext.Items["AccessLevelConfiguration"] = accessLevelConfiguration;
            User user = SecurityFunctions.CheckCookies(db, HttpContext);
            if (!SecurityFunctions.HasAccessTo(pageID, user, HttpContext))
                return RedirectToAction(nameof(AdminPanel));

            HttpContext.Items["User"] = user;

            switch (pageID)
            {
                case AdminPanelPages.AddProductImage:
                    ImagesManagementFunctions.AddProductImageToServer(db, uploadedFile, itemID, HttpContext, out bool successfullyUploadedImage);
                    if (successfullyUploadedImage) return StatusCode(200);
                    else return StatusCode(415);

                case AdminPanelPages.UploadFile:
                    FileManagerManagementFunctions.UploadFileToServer(path, uploadedFile, HttpContext, out bool successfullyUploadedFile);
                    if (successfullyUploadedFile) return StatusCode(200);
                    else return StatusCode(415);

                default:
                    return RedirectToAction(nameof(AdminPanel));
            }
        }
    }
}