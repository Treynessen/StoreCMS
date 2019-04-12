using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Treynessen.Database.Entities
{
    public class ConnectedUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string UserName { get; set; }
        [Required]
        public string LoginKey { get; set; }
        [Required]
        public DateTime LastActionTime { get; set; }
        // Сделать Required
        public string IPAdress { get; set; }
        [Required]
        public string UserAgent { get; set; }
        [Required]
        public User User { get; set; }
    }
}
