using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static int GetDatabaseRawID<T>(DbSet<T> table) where T : class, Interfaces.IKeyID
        {
            int ID = 1;
            foreach (var id in table.AsNoTracking().Select(t => t.ID).ToArray())
            {
                if (id != ID)
                    break;
                ++ID;
            }
            return ID;
        }
    }
}