namespace Treynessen.Localization
{
    public interface ILoginFormLocalization
    {
        string Title { get; }
        string UserName { get; }
        string UserNameError { get; }
        string Password { get; }
        string PasswordError { get; }
        string IncorrectInput { get; }
        string Enter { get; }
    }
}