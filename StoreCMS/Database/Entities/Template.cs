using System.ComponentModel.DataAnnotations;

namespace Treynessen.Database.Entities
{
    public class Template : Interfaces.ITemplate
    {
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        public string TemplateSource { get; set; }
        [Required]
        public string TemplatePath { get; set; }
    }
}