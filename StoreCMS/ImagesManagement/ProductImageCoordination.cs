using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Security.Cryptography;

namespace Treynessen.ImagesManagement
{
    public static partial class ProductImageOperations
    {
        // [Комментарий ИИ-агента]: Рекурсивный Mutex объединяет процессы одного сеанса ОС; между серверами координации нет.
        public static IDisposable Acquire(string folder)
        {
            string key = Path.GetFullPath(folder).TrimEnd('/', '\\');
            if (Path.DirectorySeparatorChar == '\\') key = key.ToUpperInvariant();
            var mutex = new Mutex(false, "StoreCMS.Images." + Hash(Encoding.UTF8.GetBytes(key)));
            try
            {
                try { mutex.WaitOne(); }
                catch (AbandonedMutexException) { }
                return new Lease(mutex);
            }
            catch { mutex.Dispose(); throw; }
        }

        private sealed class Lease : IDisposable
        {
            private Mutex mutex;
            public Lease(Mutex mutex) { this.mutex = mutex; }
            public void Dispose()
            {
                if (mutex == null) return;
                mutex.ReleaseMutex();
                mutex.Dispose();
                mutex = null;
            }
        }

        private static string Hash(byte[] bytes)
        {
            using (var sha = SHA256.Create()) return Convert.ToBase64String(sha.ComputeHash(bytes)).Replace('/', '_');
        }

        private static string VersionPath(string folder)
        {
            var directory = new DirectoryInfo(Path.GetFullPath(folder));
            if (directory.Parent?.Name != "products" || directory.Parent.Parent?.Name != "images"
                || directory.Parent.Parent.Parent?.Name != "Storage") return null;
            return Path.Combine(directory.Parent.Parent.Parent.Parent.FullName, "App_Data", "image-versions", directory.Name + ".version");
        }

        private static string GetVersion(string folder)
        {
            string path = VersionPath(folder);
            if (path == null) throw new InvalidOperationException("Неподдерживаемый каталог изображений товара.");
            if (!File.Exists(path)) Invalidate(folder);
            return File.ReadAllText(path);
        }

        // [Комментарий ИИ-агента]: Версия меняется до мутации, поэтому возврат к тем же именам/байтам не оживляет старую форму; сбой даёт безопасный конфликт.
        public static void Invalidate(string folder)
        {
            string path = VersionPath(folder);
            if (path == null) return;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, Guid.NewGuid().ToString("N"));
        }
    }
}
