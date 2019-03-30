using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trane.Db.Entities
{
    [Table("CategoryPages")]
    public class CategoryPage : Page
    {
        public SimplePage PreviousPage { get; set; }
        [Required]
        public ProductType ProductType { get; set; }
    }
}