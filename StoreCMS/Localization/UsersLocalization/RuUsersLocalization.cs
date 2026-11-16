namespace Treynessen.Localization
{
    public class RuUsersLocalization : IUsersLocalization
    {
        public string PageName => "Пользователи";

        public string Login => "Логин";
        public string Password => "Пароль";
        public string UserType => "Тип пользователя";

        public string UserActions => "Действия пользователя";
        public string Add => "Добавить";
        public string Edit => "Изменить";
        public string Delete => "Удалить";

        public string UserAdded => "Пользователь добавлен";
        public string UserEdited => "Пользователь изменен";
        public string UserDeleted => "Пользователь удален";
        public string UserNotFound => "Пользователь не найден";
        public string IncorrectInput => "Обязательные поля не заполнены или содержат недопустимые символы";
        public string PasswordTooShort => "Пароль слишком короткий";
        public string UserWithSameLoginAlreadyExists => "Пользователь с таким логином уже существует";
    }
}