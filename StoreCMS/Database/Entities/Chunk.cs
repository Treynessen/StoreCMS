using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Treynessen.Database.Entities
{
    public class Chunk : Interfaces.ITemplate
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        public string TemplateSource { get; set; }
        [Required]
        public string TemplatePath { get; set; }
    }
}
