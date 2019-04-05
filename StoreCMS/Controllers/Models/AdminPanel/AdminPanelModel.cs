using Trane.Db.Entities;
using Trane.Db.Entities.TypesForEntities;

namespace Trane.Controllers.Models
{
    public class AdminPanelModel
    {
        public AdminPanelPages PageId { get; set; }
        public SimplePage SimplePage { get; set; }
    }
}
