using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Treynessen.Database.Entities
{
    public class Redirection : Interfaces.IKeyID
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        [Required]
        public int RequestPathHash { get; set; } // Хэш-значение RequestPath'а
        [Required]
        public string RequestPath { get; set; } // Путь запроса
        [Required]
        public string RedirectionPath { get; set; }
    }
}