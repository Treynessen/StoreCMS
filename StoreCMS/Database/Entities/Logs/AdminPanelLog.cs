using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Treynessen.Database.Entities
{
    [Table("AdminPanelLogs")]
    public class AdminPanelLog : Log
    {
        [Required]
        public User User { get; set; }
        public int UserId { get; set; }
    }
}