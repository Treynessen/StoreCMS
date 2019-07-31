using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Treynessen.Functions;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.Database.Interfaces;

namespace Treynessen.TemplatesManagement
{
    public static partial class TemplatesManagementFunctions
    {
        public static void SetUniqueITemplateName(CMSDatabase db, ITemplate template)
        {
            var rawIdsAndNames = template is Template
                ? db.Templates.AsNoTracking().Where(t => t.ID != template.ID).Select(t => new { t.ID, t.Name }).ToList()
                : db.Chunks.AsNoTracking().Where(tc => tc.ID != template.ID).Select(tc => new { tc.ID, tc.Name }).ToList();
            if (rawIdsAndNames == null)
                throw new ArgumentException();

            int index = 0;
            bool has = false;
            do
            {
                has = false;
                string name = $"{template.Name}{(index == 0 ? string.Empty : index.ToString())}";
                has = rawIdsAndNames.FirstOrDefault(t => t.Name.Equals(name, StringComparison.Ordinal)) != null;
                if (has && index == 0)
                {
                    template.Name = OtherFunctions.GetNameWithUnderscore(template.Name);
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