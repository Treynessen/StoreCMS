using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Treynessen.Security
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ProductImageUploadLimitAttribute : Attribute, IAsyncResourceFilter
    {
        public const long FileLimit = 33_554_432;
        public const long EnvelopeLimit = 65_536;
        public const long BodyLimit = FileLimit + EnvelopeLimit;

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            var request = context.HttpContext.Request;
            // [Комментарий ИИ-агента]: Атрибут стоит только на выбранном MVC PUT-action; тело не участвует в выборе исключения.
            if (!HttpMethods.IsPut(request.Method) || !request.Query.TryGetValue("pageID", out var selector)
                || selector.Count != 1 || selector[0] != "24")
            {
                await next();
                return;
            }

            var feature = context.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
            // [Комментарий ИИ-агента]: Если тело уже читалось, безопасно повысить транспортный предел невозможно.
            if (feature == null || feature.IsReadOnly || request.Body.CanSeek && request.Body.Position != 0)
            {
                context.Result = new StatusCodeResult(400);
                return;
            }
            feature.MaxRequestBodySize = BodyLimit;
            if (request.ContentLength > BodyLimit)
            {
                context.Result = new StatusCodeResult(413);
                return;
            }

            try
            {
                if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var contentType)
                    || !contentType.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException();
                string boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;
                if (string.IsNullOrEmpty(boundary) || boundary.Length > 128)
                    throw new InvalidDataException();

                // [Комментарий ИИ-агента]: Ограниченное буферизование учитывает также preamble/epilogue и chunked без Content-Length.
                request.EnableBuffering(bufferThreshold: 65_536, bufferLimit: BodyLimit + 1);
                byte[] buffer = new byte[8192];
                long total = 0;
                int read;
                while ((read = await request.Body.ReadAsync(buffer.AsMemory(0, (int)Math.Min(buffer.Length, BodyLimit - total + 1)), request.HttpContext.RequestAborted)) != 0)
                {
                    total += read;
                    if (total > BodyLimit)
                    {
                        context.Result = new StatusCodeResult(413);
                        return;
                    }
                }
                request.Body.Position = 0;
                var reader = new MultipartReader(boundary, request.Body)
                {
                    HeadersCountLimit = 16,
                    HeadersLengthLimit = (int)EnvelopeLimit,
                    BodyLengthLimit = BodyLimit
                };
                var section = await reader.ReadNextSectionAsync(request.HttpContext.RequestAborted);
                if (section == null)
                {
                    context.Result = new StatusCodeResult(415);
                    return;
                }
                if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var disposition)
                    || !disposition.DispositionType.Equals("form-data", StringComparison.OrdinalIgnoreCase)
                    || HeaderUtilities.RemoveQuotes(disposition.Name).Value != "uploadedFile"
                    || (!disposition.FileName.HasValue && !disposition.FileNameStar.HasValue)
                    || !section.BaseStreamOffset.HasValue)
                    throw new InvalidDataException();

                long length = 0;
                while ((read = await section.Body.ReadAsync(buffer.AsMemory(), request.HttpContext.RequestAborted)) != 0)
                    length += read;
                if (length > FileLimit || length == 0)
                {
                    context.Result = new StatusCodeResult(415);
                    return;
                }
                // [Комментарий ИИ-агента]: Только одна файловая секция, без form fields; весь overhead проверяется по фактическим байтам.
                if (await reader.ReadNextSectionAsync(request.HttpContext.RequestAborted) != null
                    || total - length > EnvelopeLimit)
                    throw new InvalidDataException();

                string filename = HeaderUtilities.RemoveQuotes(disposition.FileNameStar.HasValue
                    ? disposition.FileNameStar : disposition.FileName).Value;
                var file = new FormFile(request.Body, section.BaseStreamOffset.Value, length, "uploadedFile", filename)
                {
                    Headers = new HeaderDictionary(section.Headers)
                };
                request.Body.Position = 0;
                request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(),
                    new FormFileCollection { file });
            }
            catch (InvalidDataException)
            {
                context.Result = new StatusCodeResult(400);
                return;
            }
            // [Комментарий ИИ-агента]: Ошибки транспорта/диска не маскируются как ошибки формата; guards и action исполняет прежний MVC pipeline.
            await next();
        }
    }
}
