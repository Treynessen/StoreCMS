// Minimal in-memory boundaries: real rendering/chunk methods, no application host or EF provider.
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
namespace Microsoft.AspNetCore.Hosting { public interface IHostingEnvironment { string Root { get; } } }
namespace Microsoft.AspNetCore.Http
{
    public class HttpContext
    {
        public IServiceProvider RequestServices { get; set; }
        public Dictionary<string, object> Items { get; } = new();
    }
}
namespace Microsoft.Extensions.DependencyInjection
{
    public static class Services
    {
        public static T GetService<T>(this IServiceProvider p) => (T)p.GetService(typeof(T));
        public static T GetRequiredService<T>(this IServiceProvider p) => p.GetService<T>();
    }
}
namespace Microsoft.EntityFrameworkCore
{
    public enum EntityState { Detached }
    public class Entry { public EntityState State { get; set; } }
    public static class Queries
    {
        public static IEnumerable<T> AsNoTracking<T>(this IEnumerable<T> items) => items;
        public static Task<T> FirstOrDefaultAsync<T>(this TestSet<T> items, Func<T,bool> predicate) where T : Template
        {
            items.Owner.BeforeQuery?.Invoke();
            if (Interlocked.Increment(ref items.Owner.ActiveQueries) != 1) throw new Exception("Concurrent context access");
            return Task.Run(() => { try { Thread.Sleep(1); return items.FirstOrDefault(predicate); } finally { Interlocked.Decrement(ref items.Owner.ActiveQueries); } });
        }
    }
}
namespace Treynessen.Database.Entities
{
    public class Template { public int ID { get; set; } public string Name { get; set; } public string TemplateSource { get; set; } public string TemplatePath { get; set; } }
    public class Chunk : Template { }
}
namespace Treynessen.Database.Context
{
    public class TestSet<T> : List<T> where T : Template
    {
        public CMSDatabase Owner { get; }
        public TestSet(CMSDatabase owner) { Owner=owner; }
        public void Update(T item) { RemoveAll(x=>x.ID==item.ID); Add(item); }
    }
    public class CMSDatabase
    {
        public TestSet<Chunk> Chunks { get; }
        public TestSet<Template> Templates { get; }
        public int ActiveQueries;
        public Action BeforeQuery { get; set; }
        public CMSDatabase() { Chunks=new(this); Templates=new(this); }
        public Microsoft.EntityFrameworkCore.Entry Entry(object item) => new();
        public void SaveChanges() { int next=Chunks.Select(x=>x.ID).DefaultIfEmpty(0).Max()+1; foreach(var x in Chunks.Where(x=>x.ID==0)) x.ID=next++; }
    }
}
namespace Treynessen.Extensions
{
    public static class Paths
    {
        public static string GetChunksFolderShortPath(this Microsoft.AspNetCore.Hosting.IHostingEnvironment e)=>"~/Views/Chunks/";
        public static string GetChunksFolderFullPath(this Microsoft.AspNetCore.Hosting.IHostingEnvironment e)=>e.Root+"/Views/Chunks/";
        public static string GetTemplatesFolderFullPath(this Microsoft.AspNetCore.Hosting.IHostingEnvironment e)=>e.Root+"/Views/Templates/";
        public static string GetProductBlockTemplateFullPath(this Microsoft.AspNetCore.Hosting.IHostingEnvironment e)=>e.Root+"/Settings/product_block.txt";
        public static string GetProductBlockCshtmlFullPath(this Microsoft.AspNetCore.Hosting.IHostingEnvironment e)=>e.Root+"/Settings/product_block.cshtml";
        public static string GetProductBlockCshtmlShortPath(this Microsoft.AspNetCore.Hosting.IHostingEnvironment e)=>"~/Settings/product_block.cshtml";
        public static string GetProductsImagesFolderSrc(this Microsoft.AspNetCore.Hosting.IHostingEnvironment e)=>"/images/products/";
    }
}
namespace Treynessen.AdminPanelTypes { public class TemplateModel { public int? ID { get; set; } public string Name { get; set; } public string TemplateSource { get; set; } } }
namespace Treynessen.Localization { public interface IAdminPanelLogLocalization { string ChunkAdded { get; } string ChunkEdited { get; } string ChunkDeleted { get; } } }
namespace Treynessen.LogManagement { public static class LogManagementFunctions { public static void AddAdminPanelLog(CMSDatabase db, Microsoft.AspNetCore.Http.HttpContext context, string info) { } } }
namespace Treynessen.TemplatesManagement
{
    public static partial class TemplatesManagementFunctions
    {
        public static T TemplateModelToITemplate<T>(Treynessen.AdminPanelTypes.TemplateModel model, Microsoft.AspNetCore.Http.HttpContext context) where T:Template,new()=>new T { Name=model.Name, TemplateSource=model.TemplateSource };
        public static void SetUniqueITemplateName(CMSDatabase db, Template item) { }
    }
}
