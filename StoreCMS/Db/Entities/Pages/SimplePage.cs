using System.ComponentModel.DataAnnotations.Schema;

namespace Trane.Db.Entities
{
    // Сущность для простых страниц. Для запросов типа localhost/directory
    // сначала поиск будет вестись по таблице SimplePages.
    [Table("SimplePages")]
    public class SimplePage : Page
    {
        public SimplePage PreviousPage { get; set; }
    }
}
