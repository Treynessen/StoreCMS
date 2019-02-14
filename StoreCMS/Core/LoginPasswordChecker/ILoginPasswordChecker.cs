namespace Trane.AdminPanel
{
    public interface ILoginPasswordChecker
    {
        string Login { set; }
        string Password { set; }

        bool Check();
    }
}