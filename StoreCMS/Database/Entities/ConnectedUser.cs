using System;
using System.ComponentModel.DataAnnotations;

namespace Treynessen.Database.Entities
{
    public class ConnectedUser
    {
        public int ID { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string LoginKey { get; set; }
        [Required]
        public DateTime LastActionTime { get; set; }
        [Required]
        public string IPAdress { get; set; }
        [Required]
        public string UserAgent { get; set; }
        [Required]
        public User User { get; set; }
        public int UserID { get; set; }
    }
}