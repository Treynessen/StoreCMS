using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.Database.Context;

namespace Treynessen.ImagesManagement
{
    public static partial class ImagesManagementFunctions
    {
        public static int DeleteProductImage(CMSDatabase db, int? productID, string imageName, string imageToken, HttpContext context)
        {
            if (!productID.HasValue) return 404;
            var env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
            string folder = $"{env.GetProductsImagesFolderFullPath()}{productID.Value}/";
            using (ProductImageOperations.Acquire(folder))
            {
                var product = db.ProductPages.AsNoTracking().FirstOrDefault(p => p.ID == productID.Value);
                if (product == null) return 404;
                var provider = context.RequestServices.GetRequiredService<IDataProtectionProvider>();
                if (!ProductImageOperations.MatchesToken(product, folder, imageName, imageToken, provider)) return 409;
                DeleteImage(folder, imageName, db, env);
                var remaining = ProductImageOperations.GetOriginalNames(folder, product.Alias);
                for (int i = 0; i < remaining.Length; i++)
                {
                    string target = product.Alias + (i == 0 ? "" : "_" + (i + 1)) + ".jpg";
                    if (remaining[i] == target) continue;
                    RenameImageAndDependencies(db, env, folder, Path.GetFileNameWithoutExtension(remaining[i]),
                        Path.GetFileNameWithoutExtension(target), ".jpg");
                }
                LogManagementFunctions.AddAdminPanelLog(db, context,
                    $"{product.PageName} (ID-{product.ID}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.ProductImageDeleted}");
                return 200;
            }
        }
    }
}
