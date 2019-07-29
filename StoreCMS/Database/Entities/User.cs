using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Treynessen.Database.Entities
{
    public class User
    {
        public int ID { get; set; }
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
        public string Email { get; set; }
        [Range(10, 10080)]
        public uint IdleTime { get; set; }
        [Required]
        public UserType UserType { get; set; }
        public int UserTypeID { get; set; }

        public List<AdminPanelLog> AdminPanelLogs { get; set; }
    }
}