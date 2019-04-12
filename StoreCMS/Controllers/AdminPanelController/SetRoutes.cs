using Microsoft.AspNetCore.Mvc;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        private void SetRoutes(string name)
        {
            if (string.IsNullOrEmpty(HttpContext.Items["Routes"] as string))
                HttpContext.Items["Routes"] = name;
            else HttpContext.Items["Routes"] += $" -> {name}";
        }
    }
}
