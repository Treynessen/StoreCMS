using System.Linq;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Context;

namespace Treynessen.Database;

public static class SystemObjectProtection
{
    public static bool IsSystemUser(CMSDatabase db, int id)
    {
        var marker = db.SecurityInstallation.AsNoTracking().SingleOrDefault();
        return marker?.SchemaVersion != 3 || !marker.SystemUserId.HasValue || marker.SystemUserId == id;
    }
    public static bool IsSystemType(CMSDatabase db, int id)
    {
        var marker = db.SecurityInstallation.AsNoTracking().SingleOrDefault();
        return marker?.SchemaVersion != 3 || !marker.SystemUserTypeId.HasValue || marker.SystemUserTypeId == id;
    }
}
