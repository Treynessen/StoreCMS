using System.ComponentModel.DataAnnotations;
using Treynessen.PagesManagement;

namespace Treynessen.Database.Entities
{
    public class VisitedPage
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public Visitor Visitor { get; set; }
        public int VisitorId { get; set; }
        public int VisitedPageId { get; set; }
        [Required]
        public PageType PageType { get; set; }
    }
}