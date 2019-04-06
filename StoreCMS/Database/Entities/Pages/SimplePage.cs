using System.ComponentModel.DataAnnotations.Schema;

namespace Trane.Database.Entities
{
    [Table("SimplePages")]
    public class SimplePage : Page
    {
        public SimplePage PreviousPage { get; set; }
    }
}
