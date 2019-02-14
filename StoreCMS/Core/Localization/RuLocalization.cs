namespace Trane.Localization
{
    public class RuLocalization : ILocalization
    {
        // ДЛЯ СТРАНИЦЫ 404
        public string PageNotFoundText => "Страница не найдена";

        // ДЛЯ ПАНЕЛИ АДМИНИСТРАТОРА
        // ЛОГИН ФОРМА
        public string AdminPanelTitle => "Панель администратора";
        public string UserNameLabel => "Имя пользователя";
        public string PasswordLabel => "Пароль";
        public string LoginButtonText => "Войти";
        public string IncorrectLoginPassword
            => "Неправильное имя пользователя или пароль. Пожалуйста, проверьте введённые данные!";
    }
}