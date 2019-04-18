using System.Linq;
using System.Collections.Generic;
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
                    TemplateChunk chunk = db.TemplateChunks.FirstOrDefault(tc => tc.Name.Equals(chunkName));
                    if (chunk != null)
                        chunks.AddLast(chunk);
                }
            }
            return chunks;
        }
    }
}
