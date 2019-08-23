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
            string[] productImages = new string[0];
            if (product != null)
            {
                string imagesPath = $"{env.GetProductsImagesFolderFullPath()}{product.ID}/";
                if (Directory.Exists(imagesPath))
                {
                    productImages = Directory.GetFiles(imagesPath, $"*{product.Alias}*.jpg");
                    if (productImages.Length > 0)
                    {
                        Regex regex = new Regex($"{product.Alias}(_(\\d)+)?.jpg$");
                        productImages = (from img in productImages
                                         where regex.IsMatch(img)
                                         let imageNameEnding = regex.Match(img).Value.Substring(product.Alias.Length)
                                         orderby imageNameEnding.Length == 4 ? 0 : Convert.ToInt32(imageNameEnding.Substring(1, imageNameEnding.IndexOf('.') - 1))
                                         select img.Substring(env.GetStorageFolderFullPath().Length - 1)).Skip(skipImages).ToArray();
                    }
                }
            }
            return productImages;
        }
    }
}