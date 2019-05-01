using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static LinkedList<TemplateChunk> GetChunks(CMSDatabase db, string source, string currentChunkName = null)
        {
            LinkedList<TemplateChunk> chunks = new LinkedList<TemplateChunk>();
            int pointer = 0;
            while (pointer != -1)
            {
                pointer = GetNextChunk(pointer, source, out string chunkName);
                if (!string.IsNullOrEmpty(chunkName) && !chunkName.Equals(currentChunkName))
                {
                    TemplateChunk chunk = db.TemplateChunks.FirstOrDefaultAsync(tc => tc.Name.Equals(chunkName)).Result;
                    if (chunk != null)
                    {
                        db.Entry(chunk).State = EntityState.Detached;
                        chunks.AddLast(chunk);
                    }
                }
            }
            return chunks;
        }
    }
}
