namespace Treynessen.Localization
{
    public interface IUsersLocalization
    {
        string PageName { get; }

        string Login { get; }
        string Password { get; }
        string UserType { get; }

        string UserActions { get; }
        string Add { get; }
        string Edit { get; }
        string Delete { get; }

        string UserAdded { get; }
        string UserEdited { get; }
        string UserDeleted { get; }
        string UserNotFound { get; }
        string IncorrectInput { get; }
        string PasswordTooShort { get; }
        string UserWithSameLoginAlreadyExists { get; }
    }
}