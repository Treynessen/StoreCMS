using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Treynessen.Database.Entities
{
    public abstract class Page : Interfaces.IKeyID
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        [Required]
        public int RequestPathHash { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string PageName { get; set; }
        [Required]
        public string Alias { get; set; } // Url псевдоним страницы
        [Required]
        public string RequestPath { get; set; } // Путь запроса: /.../Alias
        public string BreadcrumbsHtml { get; set; } // Навигационное меню в виде html кода без названия текущей страницы
        public int? TemplateId { get; set; }
        public Template Template { get; set; } // Шаблон страницы
        public string Content { get; set; } // Содержимое страницы
        public bool Published { get; set; } // true, если страница опубликована (является видимой для пользователей)
        public string PageDescription { get; set; } // Описание для мета тега description
        public string PageKeywords { get; set; } // Ключевые слова для мета тега keywords
        public bool IsIndex { get; set; } // Доступна для индексации
        public bool IsFollow { get; set; } // Доступны переходы по ссылкам, расположенным на странице
    }
}