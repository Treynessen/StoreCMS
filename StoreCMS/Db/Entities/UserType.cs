using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Trane.Db.Entities
{
    public class UserType
    {
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }

        public List<User> Users { get; set; }
    }
}
