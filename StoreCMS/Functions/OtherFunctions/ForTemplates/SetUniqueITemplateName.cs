using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.Database.Interfaces;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static void SetUniqueITemplateName(CMSDatabase db, ITemplate template)
        {
            var rawsIdAndName = template is Template 
                ? db.Templates.Where(t => t.ID != template.ID).AsNoTracking().Select(t => new { t.ID, t.Name }).ToList()
                : db.Chunks.Where(tc => tc.ID != template.ID).AsNoTracking().Select(tc => new { tc.ID, tc.Name }).ToList();
            if (rawsIdAndName == null)
                throw new ArgumentException();

            int index = 0;
            bool has = false;
            do
            {
                has = false;
                string name = $"{template.Name}{(index == 0 ? "" : index.ToString())}";
                has = rawsIdAndName.FirstOrDefault(t => t.Name.Equals(name, StringComparison.InvariantCulture)) != null;
                if (has && index == 0)
                {
                    template.Name = GetNameWithUnderscore(template.Name);
                }
                if (!has)
                {
                    template.Name = name;
                }
                ++index;
            } while (has);
        }
    }
}