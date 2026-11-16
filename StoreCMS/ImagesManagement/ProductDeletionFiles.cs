using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Treynessen.Extensions;

namespace Treynessen.ImagesManagement
{
    internal sealed class ProductDeletionFiles : IDisposable
    {
        private readonly IWebHostEnvironment environment;
        private readonly string staging;
        private readonly List<IDisposable> leases = new List<IDisposable>();
        private readonly List<(string Source, string Staged)> moved = new List<(string, string)>();

        public ProductDeletionFiles(IWebHostEnvironment environment)
        {
            this.environment = environment;
            staging = Path.Combine(environment.ContentRootPath, "App_Data", "product-deletions", Guid.NewGuid().ToString("N"));
        }

        public void Stage(int productId)
        {
            string source = Path.Combine(environment.GetProductsImagesFolderFullPath(), productId.ToString());
            leases.Add(ProductImageOperations.Acquire(source));
            if (!Directory.Exists(source)) return;
            Directory.CreateDirectory(staging);
            string target = Path.Combine(staging, productId.ToString());
            // [Комментарий ИИ-агента]: До подтверждённого COMMIT каталог только перемещаем в непубличную область того же тома.
            Directory.Move(source, target);
            moved.Add((source, target));
        }

        public void Restore(Exception failure)
        {
            var errors = new List<Exception> { failure };
            for (int i = moved.Count - 1; i >= 0; --i)
            {
                try { Directory.Move(moved[i].Staged, moved[i].Source); }
                catch (Exception error) when (error is IOException || error is UnauthorizedAccessException) { errors.Add(error); }
            }
            if (errors.Count > 1)
                throw new AggregateException("Не завершено восстановление каталогов после отката удаления; сохраните App_Data/product-deletions и сверьте БД.", errors);
            if (Directory.Exists(staging)) Directory.Delete(staging);
        }

        public void DeleteCommitted()
        {
            try { if (Directory.Exists(staging)) Directory.Delete(staging, true); }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            {
                throw new IOException("Удаление подтверждено в БД; требуется очистка оставшегося каталога App_Data/product-deletions. Не повторяйте удаление вслепую.", error);
            }
        }

        public void Dispose()
        {
            // [Комментарий ИИ-агента]: Dispose освобождает только блокировки; неизвестный исход COMMIT не разрешает компенсацию файлов.
            for (int i = leases.Count - 1; i >= 0; --i) leases[i].Dispose();
        }
    }
}
