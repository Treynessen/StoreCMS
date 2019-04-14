using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Treynessen.AdminPanelTypes
{
    public class AdminPanelModel
    {
        public AdminPanelPages? PageId { get; set; }
        public PageType? PageType { get; set; }
        public int? itemID { get; set; }

        public PageModel PageModel { get; set; }
        public TemplateModel TemplateModel { get; set; }
    }
}
