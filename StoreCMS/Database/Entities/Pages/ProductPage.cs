using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Treynessen.Database.Entities
{
    [Table("ProductPages")]
    public class ProductPage : Page
    {
        public uint Price { get; set; }
        public uint OldPrice { get; set; }
        public string ShortDescription { get; set; } // Краткое описание товара
        public bool SpecialProduct { get; set; } // Особый товар (хит продаж или что-то типо этого)
        public DateTime LastUpdate { get; set; }

        [Required]
        public CategoryPage PreviousPage { get; set; }
        public int PreviousPageID { get; set; }
    }
}