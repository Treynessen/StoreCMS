namespace Trane.AdminPanel
{
    public class LoginPasswordChecker : ILoginPasswordChecker
    {
        public string Login { set => login = value; get => null; }
        public string Password { set => password = value; get => null; }

        private string login, password;

        public bool Check()
        {
            if (login == "admin" && password == "admin")
                return true;
            return false;
        }
    }
}