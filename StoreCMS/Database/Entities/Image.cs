using System.ComponentModel.DataAnnotations;

namespace Treynessen.Database.Entities
{
    public class Image
    {
        public int ID { get; set; }
        [Required]
        public string ShortPath { get; set; } // путь от папки Storage
        [Required]
        public int ShortPathHash { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public uint Width { get; set; }
        [Required]
        public uint Height { get; set; }
    }
}