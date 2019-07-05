using System.Threading;
using System.Threading.Tasks;
using Treynessen.Database.Entities;

namespace Treynessen.RequestManagement
{
    public partial class RequestHandler
    {
        public Page FindPage()
        {
            string _requestString = requestString;
            if (_requestString.Length > 1 && _requestString[0].Equals('/'))
                _requestString = _requestString.Substring(1);
            string[] splitedRequestString = _requestString.Split('/');

            CancellationTokenSource productPageSearchTokenSource = null;
            CancellationTokenSource categoryPageSearchTokenSource = new CancellationTokenSource();
            CancellationTokenSource usualPageSearchTokenSource = new CancellationTokenSource();

            Task<ProductPage> productPageSearchTask = null;
            if (splitedRequestString.Length != 1)
            {
                productPageSearchTokenSource = new CancellationTokenSource();
                productPageSearchTask = FindPageInTable(db.ProductPages, productPageSearchTokenSource, categoryPageSearchTokenSource, usualPageSearchTokenSource);
            }

            Task<CategoryPage> categoryPageSearchTask = FindPageInTable(db.CategoryPages, categoryPageSearchTokenSource, productPageSearchTokenSource, usualPageSearchTokenSource);
            Task<UsualPage> usualPageSearchTask = FindPageInTable(db.UsualPages, usualPageSearchTokenSource, productPageSearchTokenSource, categoryPageSearchTokenSource);

            if (productPageSearchTask != null && productPageSearchTask.Result != null)
                return productPageSearchTask.Result;
            else if (categoryPageSearchTask.Result != null)
                return categoryPageSearchTask.Result;
            else if (usualPageSearchTask.Result != null)
                return usualPageSearchTask.Result;
            else return null;
        }
    }
}