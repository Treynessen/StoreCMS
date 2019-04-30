﻿namespace Treynessen.AdminPanelTypes
{
    public class AdminPanelModel
    {
        public AdminPanelPages? PageId { get; set; }
        public PageType? PageType { get; set; }
        public int? itemID { get; set; }

        public Microsoft.AspNetCore.Http.IFormFile uploadedFile { get; set; }
        public int? imageID { get; set; }

        public PageModel PageModel { get; set; }
        public TemplateModel TemplateModel { get; set; }
    }
}
