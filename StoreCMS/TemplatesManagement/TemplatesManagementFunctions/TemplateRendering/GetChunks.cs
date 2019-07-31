using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.TemplatesManagement
{
    public static partial class TemplatesManagementFunctions
    {
        private static LinkedList<Chunk> GetChunks(CMSDatabase db, string source, string currentChunkName = null)
        {
            LinkedList<Chunk> chunks = new LinkedList<Chunk>();
            int pointer = 0;
            while (pointer != -1)
            {
                pointer = GetNextChunk(pointer, source, out string chunkName);
                if (!string.IsNullOrEmpty(chunkName) && !chunkName.Equals(currentChunkName, StringComparison.Ordinal))
                {
                    Chunk chunk = db.Chunks.FirstOrDefaultAsync(tc => tc.Name.Equals(chunkName, StringComparison.Ordinal)).Result;
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
