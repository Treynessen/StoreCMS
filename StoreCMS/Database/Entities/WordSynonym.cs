using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Treynessen.Database.Entities
{
    public class SynonymForString
    {
        public int ID { get; set; }
        [Required]
        public string String { get; set; }
        [Required]
        public string Synonym { get; set; }
    }
}
