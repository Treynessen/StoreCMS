using System;
using Microsoft.AspNetCore.Http;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Security
{
    public static partial class SecurityFunctions
    {
        public static User CheckCookies(CMSDatabase db, HttpContext context) =>
            SessionStore.Authenticate(db, context, AdminSessionPolicy.Instance, () => DateTime.UtcNow);
    }
}
