using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.Database.Context;

namespace Treynessen.FileManagerManagement
{
    public static partial class FileManagerManagementFunctions
    {
        public static void UploadFileToServer(CMSDatabase db, string path, IFormFile file, HttpContext context, out bool successfulUpload)
        {
            successfulUpload = false;
            // [Комментарий ИИ-агента]: Отклоняем отсутствующий или пустой файл до обращения к окружению и файловой системе.
            if (file == null || file.Length <= 0 || string.IsNullOrWhiteSpace(file.FileName))
            {
                successfulUpload = false;
                return;
            }
            IWebHostEnvironment env = context.RequestServices.GetService<IWebHostEnvironment>();
            if (string.IsNullOrEmpty(path))
            {
                path = env.GetStorageFolderFullPath();
            }
            else
            {
                Regex regex = new Regex(@"^((\w|-|_)+)(>(\w|-|_)+)*$");
                if (!regex.IsMatch(path))
                {
                    successfulUpload = false;
                    return;
                }
                path = path.Replace('>', '/');
                if (!path[path.Length - 1].Equals('/'))
                    path = path.Insert(path.Length, "/");
                path = $"{env.GetStorageFolderFullPath()}{path}";
            }
            if (!Directory.Exists(path) || !HasAccessToFolder(path, env))
            {
                successfulUpload = false;
                return;
            }
            int pointIndex = file.FileName.LastIndexOf('.');
            if (pointIndex == -1)
            {
                successfulUpload = false;
                return;
            }
            string fileExtension = file.FileName.Substring(pointIndex).ToLower();
            bool itsCorrectExtension = false;
            foreach (var typeOfExtension in typesOfExtensions)
            {
                if (fileExtension.Equals(typeOfExtension.Key, StringComparison.Ordinal))
                {
                    itsCorrectExtension = true;
                    break;
                }
            }
            if (!itsCorrectExtension)
            {
                successfulUpload = false;
                return;
            }
            string fileName = file.FileName.Substring(0, pointIndex);
            fileName = OtherFunctions.GetCorrectName(fileName, context);
            if (string.IsNullOrEmpty(fileName))
                fileName = "uploaded_file";
            string temporary = Path.Combine(path, ".upload-" + Guid.NewGuid().ToString("N") + ".tmp");
            bool ownsTemporary = false;
            Exception primaryException = null;
            try
            {
                using (var fs = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    ownsTemporary = true;
                    file.CopyTo(fs);
                    fs.Flush(true);
                }
                string basis = fileName;
                for (int attempt = 0; ; attempt++)
                {
                    fileName = GetUniqueFileOrFolderName(path, basis, fileExtension);
                    try
                    {
                        File.Move(temporary, Path.Combine(path, fileName), false);
                        break;
                    }
                    catch (IOException) when (attempt < 127 && File.Exists(Path.Combine(path, fileName)))
                    {
                        // [Комментарий ИИ-агента]: Повторяем только выбор имени после доказанной коллизии; содержимое и SQL повторно не записываются.
                    }
                }
            }
            catch (Exception error)
            {
                primaryException = error;
                throw;
            }
            finally
            {
                try { if (ownsTemporary && File.Exists(temporary)) File.Delete(temporary); }
                catch (Exception cleanupError) when (primaryException != null)
                {
                    primaryException.Data["StoreCMS.Upload.CleanupException"] = cleanupError;
                }
            }

            // [Комментарий ИИ-агента]: При ошибке журнала опубликованный файл сохраняется; неизвестный SQL-исход не разрешает удаление или повтор операции.
            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{fileName}: {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.FileUploadedTo} {path.Substring(env.GetStorageFolderFullPath().Length - 1)}"
            );
            successfulUpload = true;
        }
    }
}
