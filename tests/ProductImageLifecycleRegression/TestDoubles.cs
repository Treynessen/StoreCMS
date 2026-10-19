using System;
using System.Linq;
using System.Collections.Generic;
namespace Microsoft.EntityFrameworkCore {
 public class DbUpdateException:Exception {}
 public enum EntityState {Unchanged,Modified,Deleted}
 public class TestEntry {public EntityState State {get;set;}}
 public static class Query { public static IEnumerable<T> AsNoTracking<T>(this IEnumerable<T> rows)=>rows; }
}
namespace Treynessen.Database.Entities {
 public class Image { public string ShortPath {get;set;} public int ShortPathHash {get;set;} public string FullName {get;set;} public uint Width {get;set;} public uint Height {get;set;} }
 public class ProductPage {public int ID {get;set;} public string Alias {get;set;} public string PageName {get;set;} }
}
namespace Treynessen.Database.Context {
 public class CMSDatabase {
 public List<Treynessen.Database.Entities.Image> Images {get;}=new();
 public List<Treynessen.Database.Entities.ProductPage> ProductPages {get;}=new();
 public int Saves; public Action OnSave; public int ThrowSave;
 public int SaveChanges(){Saves++; OnSave?.Invoke(); if(Saves==ThrowSave)throw new Microsoft.EntityFrameworkCore.DbUpdateException();return 1;}
 private Dictionary<object,Microsoft.EntityFrameworkCore.TestEntry> entries=new();
 public Microsoft.EntityFrameworkCore.TestEntry Entry(object x){if(!entries.ContainsKey(x))entries[x]=new();return entries[x];}
 public void Remove(Treynessen.Database.Entities.Image x){Images.Remove(x);Entry(x).State=Microsoft.EntityFrameworkCore.EntityState.Deleted;}
 }
}
namespace Treynessen.Extensions {
 public static class EnvExtensions {
 public static string GetStorageFolderFullPath(this Microsoft.AspNetCore.Hosting.IHostingEnvironment env)=>env.WebRootPath;
 public static string GetProductsImagesFolderFullPath(this Microsoft.AspNetCore.Hosting.IHostingEnvironment env)=>env.WebRootPath+"images/products/";
 }
}
namespace Treynessen.Localization {public interface IAdminPanelLogLocalization {string ProductImageUploaded {get;} string ProductImageDeleted {get;}}}
namespace Treynessen.LogManagement {public static class LogManagementFunctions {public static void AddAdminPanelLog(Treynessen.Database.Context.CMSDatabase db,Microsoft.AspNetCore.Http.HttpContext context,string info){}}}
