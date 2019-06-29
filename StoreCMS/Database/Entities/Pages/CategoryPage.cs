using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Treynessen.Database.Entities
{
    [Table("CategoryPages")]
    public class CategoryPage : Page
    {
        public int? PreviousPageID { get; set; }
        public UsualPage PreviousPage { get; set; }

        public List<ProductPage> ProductPages { get; set; }
    }
}