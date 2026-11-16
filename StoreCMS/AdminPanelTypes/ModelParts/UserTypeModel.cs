using Treynessen.Security;

namespace Treynessen.AdminPanelTypes
{
    public class UserTypeModel
    {
        public string Name { get; set; }
        public AccessLevel? AccessLevel { get; set; }
    }
}
