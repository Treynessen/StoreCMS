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

        public string RequestCannotBeCompleted => "Запрос не может быть выполнен";

        public string RedirectionAdded => "Перенаправление добавлено";
        public string RedirectionEdited => "Перенаправление изменено";
        public string RedirectionDeleted => "Перенаправление удалено";
    }
}