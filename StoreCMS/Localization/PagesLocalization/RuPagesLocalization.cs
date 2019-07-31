namespace Treynessen.Localization
{
    public class RuPagesLocalization : IPagesLocalization
    {
        public string Pages_PageName => "Страницы";
        public string AddPage_PageName => "Добавление страницы";
        public string EditPage_PageName => "Изменение страницы";

        public string AddPage => "Добавить страницу";
        public string Name => "Название";
        public string ActionsWithPage => "Действия";
        public string EditPage => "Изменить страницу";
        public string DeletePage => "Удалить страницу";

        public string SaveButton => "Сохранить";
        public string Title => "Заголовок(Title) страницы";
        public string Breadcrumb => "Название страницы";
        public string PageDescription => "Description в meta-теге";
        public string PageKeywords => "Keywords в meta-теге";
        public string Template => "Шаблон";
        public string WithoutTemplate => "Без шаблона";
        public string PreviousPage => "Страница-родитель";
        public string WithoutPreviousPage => "Независимая страница";
        public string Alias => "Псевдоним страницы";
        public string IsMainPage => "Главная страница";
        public string Published => "Опубликована";
        public string IsIndex => "Поисковым системам доступна индексация страницы";
        public string IsFollow => "Поисковым системам доступен переход по ссылкам";
        public string Content => "Содержимое страницы";

        public string PageAdded => "Страница добавлена";
        public string PageEdited => "Страница изменена";
        public string PageDeleted => "Страница удалена";
        public string PageNotFound => "Страница не найдена";
        public string IncorrectInput => "Обязательные поля (помеченные *) не заполнены или содержат недопустимые символы";
    }
}