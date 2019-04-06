using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trane.Database.Entities
{
    [Table("ProductPages")]
    public class ProductPage : Page
    {
        [Required]
        public string Name { get; set; }
        public uint Price { get; set; }
        public uint OldPrice { get; set; }
        public string Description { get; set; }

        [Required]
        public CategoryPage PreviousPage { get; set; }
    }
}
