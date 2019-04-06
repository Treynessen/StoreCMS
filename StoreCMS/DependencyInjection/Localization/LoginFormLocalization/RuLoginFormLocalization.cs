namespace Trane.DependencyInjection
{
    public class RuLoginFormLocalization : ILoginFormLocalization
    {
        public string Title => "Вход в панель управления";
        public string UserName => "Имя пользователя";
        public string UserNameError => "Не введено имя пользователя";
        public string Password => "Пароль";
        public string PasswordError => "Не введен пароль";
        public string IncorrectInput => "Неправильное имя пользователя или пароль. Пожалуйста, проверьте введённые данные и попытайтесь снова.";
        public string Enter => "Войти";
    }
}
