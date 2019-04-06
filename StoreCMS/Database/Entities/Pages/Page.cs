using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trane.Database.Entities
{
    public class Page : IKeyID
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string BreadcrumbName { get; set; }
        [Required]
        public string RequestPath { get; set; }
        public string TemplatePath { get; set; }
    }
}
