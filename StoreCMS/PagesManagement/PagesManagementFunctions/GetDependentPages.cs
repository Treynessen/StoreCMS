using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.PagesManagement
{
    public static partial class PagesManagementFunctions
    {
        public static LinkedList<int> GetDependentPageIDs(CMSDatabase db, UsualPage page, LinkedList<int> idList = null)
        {
            if (idList == null)
                idList = new LinkedList<int>();
            UsualPage[] usualPages = usualPages = db.UsualPages.Where(up => up.PreviousPageID.HasValue && up.PreviousPageID == page.ID).ToArray();
            foreach (var up in usualPages)
            {
                GetDependentPageIDs(db, up, idList);
            }
            idList.AddLast(page.ID);
            db.Entry(page).State = EntityState.Detached;
            return idList;
        }
    }
}