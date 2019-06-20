﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Treynessen.Extensions;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class ActionsWithDatabase
    {
        public static bool EditChunk(CMSDatabase db, AdminPanelModel model, HttpContext context)
        {
            if (!model.itemID.HasValue || model.TemplateModel == null)
                return false;
            Chunk changeChunk = db.Chunks.FirstOrDefaultAsync(t => t.ID == model.itemID).Result;
            db.Entry(changeChunk).State = EntityState.Detached;
            if (changeChunk == null)
                return false;
            IHostingEnvironment env = context.RequestServices.GetService<IHostingEnvironment>();
            Chunk chunk = OtherFunctions.TemplateModelToITemplate<Chunk>(model.TemplateModel, context);
            if (chunk != null)
            {
                if (string.IsNullOrEmpty(chunk.TemplatePath))
                    chunk.TemplatePath = env.GetTemplateChunksPath(true);
                chunk.ID = model.itemID.Value;
            }
            else return false;
            if (!Validator.TryValidateObject(chunk, new ValidationContext(chunk), null))
                return false;

            string pathToTemplateChunks = env.GetTemplateChunksPath();
            if (chunk.Name.Equals("_ViewImports", StringComparison.InvariantCultureIgnoreCase))
                chunk.Name = "view_imports";
            OtherFunctions.SetUniqueITemplateName(db, chunk);
            chunk.TemplatePath += $"{chunk.Name}.cshtml";

            bool isChangedName = !changeChunk.Name.Equals(chunk.Name, StringComparison.InvariantCulture);
            bool isChangedSource = string.IsNullOrEmpty(changeChunk.TemplateSource) ?
                !string.IsNullOrEmpty(chunk.TemplateSource) :
                !changeChunk.TemplateSource.Equals(chunk.TemplateSource, StringComparison.InvariantCulture);

            if (isChangedName && !isChangedSource)
            {
                try
                {
                    File.Move($"{pathToTemplateChunks}{changeChunk.Name}.cshtml", $"{pathToTemplateChunks}{chunk.Name}.cshtml");
                }
                catch (FileNotFoundException)
                {
                    OtherFunctions.SourceToCSHTML(db, pathToTemplateChunks, chunk.Name, chunk.TemplateSource, env);
                }
            }
            else if (isChangedSource)
            {
                File.Delete($"{pathToTemplateChunks}{changeChunk.Name}.cshtml");
                OtherFunctions.SourceToCSHTML(db, pathToTemplateChunks, chunk.Name, chunk.TemplateSource, env);
            }

            if (isChangedName)
            {
                var templates = db.Templates
                .Where(t => t.TemplateSource.Contains($"[#{changeChunk.Name}]") || t.TemplateSource.Contains($"[#{chunk.Name}]"))
                .AsNoTracking()
                .ToList();
                var chunks = db.Chunks
                .Where(tc => tc.ID != changeChunk.ID
                && (tc.TemplateSource.Contains($"[#{changeChunk.Name}]") || tc.TemplateSource.Contains($"[#{chunk.Name}]")))
                .AsNoTracking()
                .ToList();
                db.Chunks.Update(chunk);
                db.SaveChanges();
                var renderTask = Task.Run(() =>
                {
                    foreach (var t in templates)
                        OtherFunctions.SourceToCSHTML(db, env.GetTemplatesPath(), t.Name, t.TemplateSource, env);
                });
                foreach (var tc in chunks)
                    OtherFunctions.SourceToCSHTML(db, pathToTemplateChunks, tc.Name, tc.TemplateSource, env);
                renderTask.Wait();
            }
            else
            {
                db.Chunks.Update(chunk);
                db.SaveChanges();
            }
            return true;
        }
    }
}