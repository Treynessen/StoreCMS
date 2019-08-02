namespace Treynessen.Localization
{
    public class RuUserProfileLocalization : IUserProfileLocalization
    {
        public string Login => "Логин";
        public string NewPassword => "Новый пароль";
        public string PasswordReplay => "Повтор пароля";
        public string Email => "E-mail";
        public string CurrentPassword => "Текущий пароль";

        public string Save => "Сохранить";

        public string FieldLoginDoesntHaveValue => "Поле «Логин» не содержит значение";
        public string PasswordsDontMatch => "Пароли не совпадают";
        public string PasswordTooShort => "Пароль слишком короткий";
        public string EmailIsIncorrect => "Неверный адрес электронной почты";
        public string FieldCurrentPasswordDoesntHaveValue => "Поле «Текущий пароль» не содержит значение";
        public string LoginOrPasswordContainInvalidSymbols => "Логин или пароль содержит недопустимые символы";

        public string UserDataChanged => "Данные пользователя изменены";
    }
}