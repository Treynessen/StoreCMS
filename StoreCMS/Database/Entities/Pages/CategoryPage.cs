using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trane.Database.Entities
{
    [Table("CategoryPages")]
    public class CategoryPage : Page
    {
        [Required]
        public string Name { get; set; }

        public List<ProductPage> ProductPages { get; set; }
        public SimplePage PreviousPage { get; set; }
    }
}
