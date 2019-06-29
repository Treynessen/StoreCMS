namespace Treynessen.AdminPanelTypes
{
    public class LoginFormModel
    {
        public bool HasData => !string.IsNullOrEmpty(Login) || !string.IsNullOrEmpty(Password);

        public string Login { get; set; }
        public string Password { get; set; }
    }
}