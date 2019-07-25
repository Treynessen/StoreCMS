using Treynessen.Functions;
using Treynessen.Database.Context;

namespace Treynessen.RequestManagement
{
    public partial class RequestHandler
    {
        private string requestString;
        private int requestStringHash;
        private CMSDatabase db;

        public RequestHandler(CMSDatabase db, string requestString)
        {
            if (requestString.Length > 1 && requestString[requestString.Length - 1].Equals('/'))
                requestString = requestString.Substring(0, requestString.Length - 1);
            this.requestString = requestString.ToLower();
            requestStringHash = OtherFunctions.GetHashFromString(requestString);
            this.db = db;
        }
    }
}