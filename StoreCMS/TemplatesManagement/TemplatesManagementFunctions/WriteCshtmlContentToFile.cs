using System.IO;

namespace Treynessen.TemplatesManagement
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Threading;

    public static partial class TemplatesManagementFunctions
    {
        private sealed class TemplateFileLock
        {
            internal readonly object Gate = new object();
            internal int Users;
        }

        private static readonly Dictionary<string, TemplateFileLock> templateFileLocks =
            new Dictionary<string, TemplateFileLock>(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

        [DllImport("kernel32.dll", EntryPoint = "MoveFileExW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool MoveTemplateFile(string existingFile, string newFile, uint flags);

        public static void WriteCshtmlContentToFile(string pathToTemplatesFolder, string templateName, string chstmlContent)
        {
            WriteCshtmlContentToFile(pathToTemplatesFolder, templateName, chstmlContent, null);
        }

        internal static void WriteCshtmlContentToFile(string pathToTemplatesFolder, string templateName,
            string chstmlContent, Action<string> beforePublish)
        {
            string target = Path.GetFullPath(Path.Combine(pathToTemplatesFolder, templateName + ".cshtml"));
            TemplateFileLock fileLock;
            lock (templateFileLocks)
            {
                if (!templateFileLocks.TryGetValue(target, out fileLock))
                {
                    fileLock = new TemplateFileLock();
                    templateFileLocks.Add(target, fileLock);
                }
                fileLock.Users++;
            }
            try
            {
                lock (fileLock.Gate)
                {
                    string directory = Path.GetDirectoryName(target);
                    Directory.CreateDirectory(directory);
                    string temporary = Path.Combine(directory, "." + Guid.NewGuid().ToString("N") + ".tmp");
                    bool ownsTemporary = false;
                    Exception primaryException = null;
                    try
                    {
                        using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        {
                            ownsTemporary = true;
                            using (var writer = new StreamWriter(stream))
                                writer.Write(chstmlContent);
                        }
                        beforePublish?.Invoke(temporary);
                        // [Комментарий ИИ-агента]: Готовый файл публикуется без окна отсутствия прежней версии.
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            for (int attempt = 0; ; attempt++)
                            {
                                if (MoveTemplateFile(temporary, target, 1))
                                    break;
                                int error = Marshal.GetLastWin32Error();
                                // [Комментарий ИИ-агента]: Краткий конфликт открытого Windows-дескриптора допускает ограниченный повтор.
                                if (attempt >= 100 || (error != 5 && error != 32 && error != 33))
                                    throw new IOException("Не удалось опубликовать Razor-шаблон.", new Win32Exception(error));
                                Thread.Sleep(10);
                            }
                        }
                        else if (File.Exists(target))
                            File.Replace(temporary, target, null);
                        else
                            File.Move(temporary, target);
                    }
                    catch (Exception exception)
                    {
                        primaryException = exception;
                        throw;
                    }
                    finally
                    {
                        try
                        {
                            if (ownsTemporary && File.Exists(temporary))
                                File.Delete(temporary);
                        }
                        catch (Exception cleanupException) when (primaryException != null)
                        {
                            // [Комментарий ИИ-агента]: Сохраняем первичный стек и отдельно передаём сбой очистки с путём собственного файла.
                            primaryException.Data["StoreCMS.RazorPublication.CleanupException"] = cleanupException;
                            primaryException.Data["StoreCMS.RazorPublication.TemporaryPath"] = temporary;
                        }
                    }
                }
            }
            finally
            {
                lock (templateFileLocks)
                {
                    if (--fileLock.Users == 0)
                        templateFileLocks.Remove(target);
                }
            }
        }
    }
}
