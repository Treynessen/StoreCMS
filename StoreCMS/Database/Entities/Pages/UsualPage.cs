using System.ComponentModel.DataAnnotations.Schema;

namespace Treynessen.Database.Entities
{
    [Table("UsualPages")]
    public class UsualPage : Page
    {
        public int? PreviousPageID { get; set; }
        public UsualPage PreviousPage { get; set; }
    }
}