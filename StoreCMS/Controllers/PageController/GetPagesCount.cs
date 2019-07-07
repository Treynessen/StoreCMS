using System;
using Microsoft.AspNetCore.Mvc;

namespace Treynessen.Controllers
{
    public partial class PageController : Controller
    {
        [NonAction]
        private int GetPagesCount(int productsCount, int productsCountOnPage)
        {
            try
            {
                double value = (double)productsCount / productsCountOnPage;
                int result = (int)value;
                if (result < value) ++result;
                if (result == 0)
                    result = 1;
                return result;
            }
            catch (DivideByZeroException)
            {
                return 1;
            }
        }

    }
}