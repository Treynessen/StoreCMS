using Treynessen.PagesManagement;

namespace Treynessen.AdminPanelTypes
{
    public class PageModel
    {
        public int? ID { get; set; }
        public PageType? PageType { get; set; }
        public string Title { get; set; }
        public string PageName { get; set; }
        public string Alias { get; set; }
        public int? PreviousPageID { get; set; }

        public string Content { get; set; }
        public int? TemplateId { get; set; }
        public bool Published { get; set; }
        public string PageDescription { get; set; }
        public string PageKeywords { get; set; }
        public bool IsIndex { get; set; }
        public bool IsFollow { get; set; }

        public uint Price { get; set; }
        public uint OldPrice { get; set; }
        public string ShortDescription { get; set; }
        public bool SpecialProduct { get; set; }
        public bool AddParagraphTag { get; set; }

        public bool IsMainPage { get; set; }
    }
}