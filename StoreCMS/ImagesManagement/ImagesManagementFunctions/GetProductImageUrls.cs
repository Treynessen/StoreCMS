using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Treynessen.Extensions;
using Treynessen.Database.Entities;

namespace Treynessen.ImagesManagement
{
    public static partial class ImagesManagementFunctions
    {
        public static string[] GetProductImageUrls(ProductPage product, IHostingEnvironment env, int skipImages = 0)
        {
            if (product == null) return new string[0];
            string folder = $"{env.GetProductsImagesFolderFullPath()}{product.ID}/";
            using (ProductImageOperations.Acquire(folder))
                return ProductImageOperations.GetOriginalNames(folder, product.Alias).Skip(skipImages)
                    .Select(name => (folder + name).Substring(env.GetStorageFolderFullPath().Length - 1)).ToArray();
        }
    }
}
