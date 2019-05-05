using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Treynessen.Extensions;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static void AddProductImageToServer(CMSDatabase db, IFormFile file, int? itemID, HttpContext context)
        {
            if (!itemID.HasValue)
                return;
            if (file == null)
                return;
            ProductPage productPage = db.ProductPages.FirstOrDefaultAsync(pp => pp.ID == itemID).Result;
            if (productPage == null)
                return;
            IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
            string imagesPath = $"{env.GetProductsImagesPath()}{productPage.PreviousPageID.ToString()}{productPage.ID.ToString()}\\";
            Directory.CreateDirectory(imagesPath);
            string fileName = GetUniqueFileOrFolderName(imagesPath, GetCorrectName(productPage.PageName, context), ".jpg");
            string pathToFile = $"{imagesPath}{fileName}";
            using (Stream stream = file.OpenReadStream())
            {
                try
                {
                    using (Image<Rgba32> source = Image.Load(stream))
                    {
                        DeleteCreatedImages(imagesPath, fileName);
                        string pathToImagesInfo = $"{imagesPath}images.info";
                        string fileContent = GetFileContent(pathToImagesInfo);
                        if (!string.IsNullOrEmpty(fileContent))
                        {
                            Regex regex = new Regex($"name = {fileName}.jpg; width = \\d+; height = \\d+\n");
                            LinkedList<KeyValuePair<string, string>> listOfChanges =
                                new LinkedList<KeyValuePair<string, string>>(from match in regex.Matches(fileContent)
                                                                             select new KeyValuePair<string, string>(match.Value, string.Empty));
                            if (listOfChanges.Count > 0)
                            {
                                ReplaceContentInFile(pathToImagesInfo, listOfChanges, fileContent);
                            }
                        }
                        source.Save(pathToFile);
                    }
                }
                catch (System.NotSupportedException) { }
            }
        }
    }
}
