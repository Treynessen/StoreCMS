using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Trane.OtherTypes;

namespace Trane.Database.Entities
{
    public class UserType : IKeyID
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        public AccessLevels AccessLevel { get; set; }

        public List<User> Users { get; set; }
    }
}
