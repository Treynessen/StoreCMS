namespace Treynessen.Localization
{
    public interface IUserProfileLocalization
    {
        string Login { get; }
        string NewPassword { get; }
        string PasswordReplay { get; }
        string Email { get; }
        string CurrentPassword { get; }

        string Save { get; }

        string FieldLoginDoesntHaveValue { get; }
        string PasswordsDontMatch { get; }
        string PasswordTooShort { get; }
        string EmailIsIncorrect { get; }
        string FieldCurrentPasswordDoesntHaveValue { get; }
        string LoginOrPasswordContainInvalidSymbols { get; }

        string UserDataChanged { get; }
    }
}