using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trane.Db.Entities
{
    public class User
    {
        private string password;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password
        {
            get => password;
            set
            {
                // Хэширование с солью
                password = value;
            }
        }
        [Required]
        public UserType UserType { get; set; }
    }
}
