using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Treynessen.Database.Entities
{
    public abstract class Page : Interfaces.IKeyID
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string BreadcrumbName { get; set; } // Хлебная крошка для навигационного меню
        [Required]
        public string Alias { get; set; } // Url псевдоним страницы: RequestPath/Alias
        [Required]
        public string RequestPathWithoutAlias { get; set; } // Путь запроса без псевдонима: localhost/...previousPages...
        public string TemplatePath { get; set; } // Путь до шаблона страницы
        public string Content { get; set; } // Содержимое страницы
        public bool Published { get; set; } // true, если страница опубликована (является видимой для пользователей)
        public string PageDescription { get; set; } // Описание для мета тега description
        public string PageKeywords { get; set; } // Ключевые слова для мета тега keywords
        public bool IsRobotIndex { get; set; } // Доступна для индексации
    }
}