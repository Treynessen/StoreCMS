namespace Treynessen.Localization
{
    public interface IUserProfileLocalization
    {
        string Login { get; }
        string NewPassword { get; }
        string PasswordReplay { get; }
        string IdleTime { get; }
        string Email { get; }
        string CurrentPassword { get; }

        string Save { get; }

        string ErrorIn { get; }
        string And { get; }
        string FieldDoesntHaveValue { get; }

        string PasswordsDontMatch { get; }
        string PasswordTooShort { get; }
        string MinimumIs10Minutes { get; }
        string MaximumIs10080Minutes { get; }
        string EmailIsIncorrect { get; }

        string UserNotFound { get; }
        string CurrentPasswordIsIncorrect { get; }
        string UserWithSameLoginAlreadyExists { get; }
        string LoginOrPasswordContainInvalidSymbols { get; }

        string UserDataChanged { get; }
    }
}