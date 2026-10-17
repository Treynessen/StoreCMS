using Microsoft.AspNetCore.Mvc;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController
    {
        private IActionResult UserValidationResult(int statusCode, string error)
        {
            if (error != null && (statusCode == 422 || statusCode == 409))
                return new JsonResult(new { error }) { StatusCode = statusCode };
            return StatusCode(statusCode);
        }
    }
}
