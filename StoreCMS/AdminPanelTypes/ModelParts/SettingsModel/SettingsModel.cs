namespace Treynessen.AdminPanelTypes
{
    public class SettingsModel
    {
        public uint ValueToRunForcedGarbageCollection { get; set; }
        public string ProductBlockTemplate { get; set; }
        public int? MaxNumberOfSymbolsInSearchQuery { get; set; }
        public int? SearchPageTemplateId { get; set; }
        public int? PageNotFoundTemplateId { get; set; }
        public int? NumberOfProductsOnPage { get; set; }
        public string PaginationStyleName { get; set; }
        public AccessSettings AccessSettings { get; set; }
    }
}