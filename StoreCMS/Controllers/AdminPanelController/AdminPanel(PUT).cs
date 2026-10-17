using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Security;
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
            User user = GetAuthenticatedUser();
            if (user == null)
                return StatusCode(401);
            if (!SecurityFunctions.HasAccessTo(pageID, user, HttpContext))
                return StatusCode(403);

            HttpContext.Items["User"] = user;
            HttpContext.Items["LogLocalization"] = localization;

            switch (pageID)
            {
                case AdminPanelPages.AddProductImage:
                    ImagesManagementFunctions.UploadProductImageToServer(db, uploadedFile, itemID, HttpContext, out bool successfullyUploadedImage);
                    if (successfullyUploadedImage) return StatusCode(200);
                    else return StatusCode(415);

                case AdminPanelPages.UploadFile:
                    FileManagerManagementFunctions.UploadFileToServer(db, path, uploadedFile, HttpContext, out bool successfullyUploadedFile);
                    if (successfullyUploadedFile) return StatusCode(200);
                    else return StatusCode(415);

                default:
                    return StatusCode(400);
            }
        }
    }
}
