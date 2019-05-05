namespace Treynessen.AdminPanelTypes
{
    public class FilesViewModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public FileTypeEnum? FileType { get; set; }
        public bool? CanDelete { get; set; }
    }
}
