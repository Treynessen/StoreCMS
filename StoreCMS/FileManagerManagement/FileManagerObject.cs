namespace Treynessen.FileManagerManagement
{
    public class FileManagerObject
    {
        public string Name { get; set; }
        public string ShortPath { get; set; } // Путь от папки Storage
        public bool CanDelete { get; set; }
        public FileManagerObjectType Type { get; set; }
    }
}