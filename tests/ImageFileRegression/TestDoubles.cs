// Only the filesystem methods are under test; no EF provider or database is loaded.
using System.Collections.Generic;
using Treynessen.Database.Entities;

namespace Microsoft.AspNetCore.Hosting
{
    public interface IHostingEnvironment { string StoragePath { get; } }
}
namespace Microsoft.EntityFrameworkCore
{
    public enum EntityState { Unchanged, Modified, Deleted }
    public class TestEntry { public EntityState State { get; set; } }
}
namespace Treynessen.Extensions
{
    public static class TestEnvironmentExtensions
    {
        public static string GetStorageFolderFullPath(this Microsoft.AspNetCore.Hosting.IHostingEnvironment env) => env.StoragePath;
    }
}
namespace Treynessen.Database.Entities
{
    public class Image
    {
        public string ShortPath { get; set; }
        public int ShortPathHash { get; set; }
        public string FullName { get; set; }
    }
}
namespace Treynessen.Database.Context
{
    public class CMSDatabase
    {
        public List<Image> Images { get; } = new List<Image>();
        private readonly Dictionary<Image, Microsoft.EntityFrameworkCore.TestEntry> entries = new Dictionary<Image, Microsoft.EntityFrameworkCore.TestEntry>();
        public int Saves { get; private set; }
        public Microsoft.EntityFrameworkCore.TestEntry Entry(Image image)
        {
            if (!entries.ContainsKey(image)) entries[image] = new Microsoft.EntityFrameworkCore.TestEntry();
            return entries[image];
        }
        public void Remove(Image image) { Images.Remove(image); Entry(image).State = Microsoft.EntityFrameworkCore.EntityState.Deleted; }
        public void SaveChanges() { Saves++; }
    }
}
