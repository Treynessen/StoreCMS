using System;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static void SetUniqueTemplateName(CMSDatabase db, Template template)
        {
            int index = 0;
            bool has = false;
            do
            {
                string name = $"{template.Name}{(index == 0 ? "" : index.ToString())}";
                has = false;
                if (db.Templates.FirstOrDefaultAsync(t => t.Name.Equals(name) && t.ID != template.ID).Result != null)
                    has = true;
                if (has && index == 0)
                {
                    try
                    {
                        int lastUnderscore = template.Name.LastIndexOf('_');
                        if (lastUnderscore + 1 != template.Name.Length)
                        {
                            if (lastUnderscore == -1)
                                throw new FormatException();
                            Convert.ToInt32(template.Name.Substring(lastUnderscore + 1));
                            template.Name = template.Name.Substring(0, lastUnderscore + 1);
                        }
                    }
                    catch (FormatException)
                    {
                        template.Name += "_";
                    }
                }
                if (!has && index != 0)
                {
                    template.Name += index.ToString();
                }
                ++index;
            } while (has);
        }
    }
}
