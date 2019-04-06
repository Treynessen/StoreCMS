using Microsoft.EntityFrameworkCore;
using Trane.Database.Entities;

namespace Trane.Functions
{
    public static partial class OtherFunctions
    {
        public static int GetDatabaseRawID<T>(DbSet<T> table) where T : class, IKeyID
        {
            int ID = 1;
            foreach(var raw in table)
            {
                if (raw.ID != ID)
                    break;
                ++ID;
            }
            return ID;
        }
    }
}
