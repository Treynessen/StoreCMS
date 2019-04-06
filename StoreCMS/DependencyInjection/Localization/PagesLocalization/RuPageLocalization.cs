namespace Trane.DependencyInjection
{
    public class RuPageLocalization : IPageLocalization
    {
        public string AddPage => "Добавить страницу";

        public string Title => "Заголовок";
        public string BreadcrumbName => "Псевдоним";
        public string IncorrectInput => "Обязательные поля не заполнены или содержат недопустимые символы";
    }
}