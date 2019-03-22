using System.ComponentModel.DataAnnotations;

namespace Trane.Db.Entities
{
    public class User
    {
        private string password;

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
