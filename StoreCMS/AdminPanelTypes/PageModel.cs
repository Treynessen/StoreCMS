namespace Treynessen.AdminPanelTypes
{
    public class PageModel
    {
        public PageType? PageType { get; set; }
        public string Title { get; set; }
        public string BreadcrumbName { get; set; }
        public string Alias { get; set; }
        public int? PreviousPageID { get; set; }

        public string Content { get; set; }
        public int? TemplateId { get; set; }
        public bool Published { get; set; }
        public string PageDescription { get; set; }
        public string PageKeywords { get; set; }
        public bool IsRobotIndex { get; set; }

        public uint Price { get; set; }
        public uint OldPrice { get; set; }
        public string ShortDescription { get; set; }

        public bool IsMainPage { get; set; }
    }
}