namespace Treynessen.Localization
{
    public class RuUserProfileLocalization : IUserProfileLocalization
    {
        public string Login => "Логин";
        public string NewPassword => "Новый пароль";
        public string PasswordReplay => "Повтор пароля";
        public string IdleTime => "Время бездействия";
        public string Email => "E-mail";
        public string CurrentPassword => "Текущий пароль";

        public string Save => "Сохранить";

        public string ErrorIn => "Ошибка в";
        public string And => "и";
        public string FieldDoesntHaveValue => "Поле не содержит значение";

        public string PasswordsDontMatch => "Пароли в полях «Новый пароль» и «Повтор пароля» не совпадают";
        public string PasswordTooShort => "Пароль слишком короткий";
        public string MinimumIs10Minutes => "Минимальное значение - 10 минут";
        public string MaximumIs10080Minutes => "Максимальное значение - 10080 минут";
        public string EmailIsIncorrect => "Неверный адрес электронной почты";

        public string UserNotFound => "Пользователь не найден";
        public string CurrentPasswordIsIncorrect => "Неверный «Текущий пароль»";
        public string UserWithSameLoginAlreadyExists => "Пользователь с таким логином уже существует";
        public string LoginOrPasswordContainInvalidSymbols => "Логин или пароль содержит недопустимые символы";

        public string UserDataChanged => "Данные пользователя изменены";
    }
}