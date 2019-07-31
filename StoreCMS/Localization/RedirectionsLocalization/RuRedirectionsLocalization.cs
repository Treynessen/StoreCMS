namespace Treynessen.Localization
{
    public class RuRedirectionsLocalization : IRedirectionsLocalization
    {
        public string PageName => "Перенаправления";

        public string Link => "Ссылка";
        public string Redirection => "Перенаправление";

        public string Add => "Добавить";
        public string Edit => "Изменить";
        public string Delete => "Удалить";

        public string ErrorMsg => "Обязательные поля не заполнены или содержат недопустимые значения";
        public string RedirectionNotFound => "Перенаправление не найдено";

        public string RedirectionAdded => "Перенаправление добавлено";
        public string RedirectionEdited => "Перенаправление изменено";
        public string RedirectionDeleted => "Перенаправление удалено";
    }
}