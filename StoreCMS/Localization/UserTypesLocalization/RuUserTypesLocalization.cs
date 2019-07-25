namespace Treynessen.Localization
{
    public class RuUserTypesLocalization : IUserTypesLocalization
    {
        public string PageName => "Перенаправления";

        public string VeryLowAccessLevel => "Очень низкий уровень доступа";
        public string LowAccessLevel => "Низкий уровень доступа";
        public string MiddleAccessLevel => "Средний уровень доступа";
        public string HighAccessLevel => "Высокий уровень доступа";
        public string VeryHighAccessLevel => "Очень высокий уровень доступа";

        public string UserTypeName => "Название";
        public string AccessLevel => "Уровень доступа";

        public string Add => "Добавить";
        public string Edit => "Изменить";
        public string Delete => "Удалить";

        public string RequestCannotBeCompleted => "Запрос не может быть выполнен";

        public string UserTypeAdded => "Тип пользователя добавлен";
        public string UserTypeEdited => "Тип пользователя изменен";
        public string UserTypeDeleted => "Тип пользователя удален";
    }
}