using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trane.Db.Entities
{
    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        public int Price { get; set; }
        public int OldPrice { get; set; }
        public string Description { get; set; }

        [Required]
        public string HttpRequest { get; set; }

        [Required]
        public ProductType ProductType { get; set; }
    }
}
