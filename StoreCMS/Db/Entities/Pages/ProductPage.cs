using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trane.Db.Entities
{
    [Table("ProductPages")]
    public class ProductPage : Page
    {
        public CategoryPage PreviousPage { get; set; }
        [Required]
        public Product Product { get; set; }
    }
}