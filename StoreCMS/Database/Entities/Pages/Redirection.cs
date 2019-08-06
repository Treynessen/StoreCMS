using System.ComponentModel.DataAnnotations;

namespace Treynessen.Database.Entities
{
    public class Redirection
    {
        public int ID { get; set; }
        [Required]
        public int RequestPathHash { get; set; } // Хэш-значение RequestPath'а
        [Required]
        public string RequestPath { get; set; } // Путь запроса
        [Required]
        public string RedirectionPath { get; set; }
    }
}