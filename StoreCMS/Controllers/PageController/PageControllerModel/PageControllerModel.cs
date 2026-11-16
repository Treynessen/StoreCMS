namespace Treynessen.Controllers
{
    public class PageControllerModel
    {
        public int? Page { get; set; } // Номер страницы
        public OrderBy? Orderby { get; set; } // Сортировка
        public string Search { get; set; } // Поисковый запрос
    }
}