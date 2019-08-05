using Treynessen.Database.Entities;

namespace Treynessen.AdminPanelTypes
{
    public class UserModel
    {
        public int? ID { get; set; }
        public string Login { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public uint IdleTime { get; set; }
        public string Email { get; set; }
        public int? UserTypeId { get; set; }
    }
}