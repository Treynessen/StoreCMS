using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Xml.Linq;
using System.Xml;
using System.Collections.Generic;
using System.Threading;
using Treynessen.Database.Entities;

namespace Treynessen.LogManagement
{
    public static partial class LogManagementFunctions
    {
        public static void UserLogsToLogFile(User user, DateTime currentDay, string pathToLogsFolder)
        {
            PublishUserLogs(user, pathToLogsFolder, new Dictionary<string, int>(), CancellationToken.None);
        }

        internal static IReadOnlyList<int> PublishUserLogs(User user, string pathToLogsFolder,
            IDictionary<string, int> publications, CancellationToken token)
            =>PublishUserLogs(user,pathToLogsFolder,publications,token,CleanupObservationContext.Noop);

        internal static IReadOnlyList<int> PublishUserLogs(User user, string pathToLogsFolder,
            IDictionary<string, int> publications, CancellationToken token,CleanupObservationContext context)
        {
            var observer=CleanupObservationContext.Active(context);
            observer?.Emit(CleanupObservationKind.PublisherEntered,CleanupComponent.Publisher,()=>new CleanupIdsPayload(user.AdminPanelLogs.Select(x=>x.ID)),token);
            try
            {
            var published = new List<int>();
            string folder = Path.Combine(pathToLogsFolder, user.ID.ToString(CultureInfo.InvariantCulture));
            foreach (var day in user.AdminPanelLogs.GroupBy(log => log.Time.Date))
            {
                token.ThrowIfCancellationRequested();
                Directory.CreateDirectory(folder);
                string path = Path.Combine(folder, $"{day.Key.Day}.{day.Key.Month}.{day.Key.Year}.xml");
                var publication=observer?.Publication();
                publications.TryGetValue(path, out int count);
                if (count >= 4) throw new IOException("Достигнут предел перезаписей дневного XML; SQL сохранён.");
                var document = new XDocument(new XElement("log"));
                if (File.Exists(path))
                {
                    if (new FileInfo(path).Length > 4 * 1024 * 1024) throw new IOException("XML превышает предел чтения; SQL сохранён.");
                    using var reader = XmlReader.Create(path, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, MaxCharactersInDocument = 4 * 1024 * 1024 });
                    document = XDocument.Load(reader);
                }
                if (document.Root?.Name != "log") throw new InvalidDataException("Неверный корень XML-журнала.");
                var ids = document.Root.Elements("event").Select(e => (string)e.Attribute("id")).ToHashSet();
                foreach (var log in day.OrderBy(log => log.Time).ThenBy(log => log.ID))
                {
                    string id = log.ID.ToString(CultureInfo.InvariantCulture);
                    if (!ids.Add(id)) continue;
                    document.Root.Add(new XElement("event", new XAttribute("id", id),
                        new XAttribute("time", log.Time.ToShortTimeString()), new XElement("detail", log.Info)));
                }
                string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
                using var contents = new MemoryStream();
                document.Save(contents);
                if (contents.Length > 4 * 1024 * 1024) throw new IOException("XML превышает предел публикации; SQL сохранён.");
                var fingerprint=publication?.SerializedFile(pathToLogsFolder,path,contents);
                token.ThrowIfCancellationRequested();
                try
                {
                    // [Комментарий ИИ-агента]: Сначала сохраняем законченный архив; ID события устраняет дубли при повторе после ошибки удаления из БД.
                    using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        contents.Position = 0;
                        publication?.Emit(CleanupObservationKind.WriteStarted,CleanupComponent.Publisher,()=>fingerprint,token);
                        contents.CopyTo(stream);
                        publication?.Emit(CleanupObservationKind.WriteCompleted,CleanupComponent.Publisher,()=>fingerprint,token);
                        publication?.Emit(CleanupObservationKind.FlushStarted,CleanupComponent.Publisher,()=>fingerprint,token);
                        stream.Flush(true);
                        publication?.Emit(CleanupObservationKind.FlushCompleted,CleanupComponent.Publisher,()=>fingerprint,token);
                    }
                    token.ThrowIfCancellationRequested();
                    publication?.Emit(CleanupObservationKind.MoveStarted,CleanupComponent.Publisher,()=>fingerprint,token);
                    File.Move(temporary, path, true);
                    publication?.Emit(CleanupObservationKind.MoveCompleted,CleanupComponent.Publisher,()=>fingerprint,token);
                    publications[path] = count + 1;
                    published.AddRange(day.Select(log => log.ID));
                    publication?.Emit(CleanupObservationKind.PublicationCompleted,CleanupComponent.Publisher,()=>new CleanupIdsPayload(day.Select(x=>x.ID)),token);
                }
                catch(Exception error){publication?.Failure(CleanupObservationKind.PublisherFailed,CleanupComponent.Publisher,error,CleanupCommitKnowledge.NotAttempted,token);throw;}
                finally { if (File.Exists(temporary)) File.Delete(temporary); }
            }
            observer?.Emit(CleanupObservationKind.PublisherReturned,CleanupComponent.Publisher,()=>new CleanupIdsPayload(published),token);
            return published;
            }
            catch(Exception error){observer?.Failure(CleanupObservationKind.PublisherFailed,CleanupComponent.Publisher,error,CleanupCommitKnowledge.NotAttempted,token);throw;}
        }
    }
}
