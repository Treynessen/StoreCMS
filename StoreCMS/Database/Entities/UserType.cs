using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Treynessen.Security;

namespace Treynessen.Database.Entities
{
    public class UserType
    {
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        public AccessLevel AccessLevel { get; set; }

        public List<User> Users { get; set; }
    }
}