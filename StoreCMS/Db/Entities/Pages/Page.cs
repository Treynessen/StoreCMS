using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trane.Db.Entities
{
    public abstract class Page
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string BreadcrumbName { get; set; }
        public string TemplatePath { get; set; }
    }
}