using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Treynessen.TemplatesManagement;

internal static class Program
{
    private static int checks;
    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception(name);
        Interlocked.Increment(ref checks);
    }

    private static void Write(string folder, string name, string text, Action<string> before = null)
    {
        TemplatesManagementFunctions.WriteCshtmlContentToFile(folder, name, text, before);
    }

    private static string Read(string path)
    {
        using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
        using (var reader = new StreamReader(file)) return reader.ReadToEnd();
    }

    private static Task Run(Action action)
    {
        return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private static readonly IDictionary Locks = (IDictionary)typeof(TemplatesManagementFunctions)
        .GetField("templateFileLocks", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

    private static int Users(string target)
    {
        lock (Locks)
        {
            object entry = Locks[Path.GetFullPath(target)];
            return entry == null ? 0 : (int)entry.GetType()
                .GetField("Users", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(entry);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowPrimaryFailure(Exception exception) { throw exception; }

    private static void Main()
    {
        string root = Path.Combine(Path.GetTempPath(), "StoreCMS-E01-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            string target = Path.Combine(root, "page.cshtml");
            Write(root, "page", "Новый шаблон <b>HTML</b>\\\"\r\n");
            Check(Read(target) == "Новый шаблон <b>HTML</b>\\\"\r\n", "Создание и UTF-8");
            Check(!File.ReadAllBytes(target).Take(3).SequenceEqual(new byte[] { 239, 187, 191 }), "Нет BOM");
            Write(root, "page", "replace");
            Check(Read(target) == "replace", "Замена");
            for (int i = 0; i < 100; i++)
            {
                Write(root, "page", i.ToString());
                Check(Read(target) == i.ToString(), "Последовательная запись");
            }

            var versions = Enumerable.Range(0, 32).Select(i => "VERSION" + i + new string((char)('A' + i % 26), 128 * 1024)).ToArray();
            using (var start = new ManualResetEventSlim())
            using (var ready = new CountdownEvent(32))
            using (var firstInside = new ManualResetEventSlim())
            using (var release = new ManualResetEventSlim())
            {
                int active = 0, entered = 0;
                var tasks = versions.Select(v => Run(() =>
                {
                    ready.Signal();
                    Check(start.Wait(10000), "Общий старт writers");
                    Write(root, "page", v, temp =>
                    {
                        Check(Interlocked.Increment(ref active) == 1, "Нет одновременного входа writers одного пути");
                        try
                        {
                            if (Interlocked.Increment(ref entered) == 1)
                            {
                                firstInside.Set();
                                Check(release.Wait(10000), "Освобождение первого из 32 writers");
                            }
                        }
                        finally { Interlocked.Decrement(ref active); }
                    });
                })).ToArray();
                Check(ready.Wait(10000), "Все 32 потока готовы");
                start.Set();
                try
                {
                    Check(firstInside.Wait(10000), "Первый из 32 удерживает Gate");
                    Check(SpinWait.SpinUntil(() => Users(target) == 32, 10000), "Все 31 writer зарегистрированы при удержании первого");
                    Check(Volatile.Read(ref entered) == 1, "В Gate вошёл только первый из 32");
                }
                finally { release.Set(); }
                Check(Task.WaitAll(tasks, 20000), "32 writers завершились");
                Check(entered == 32 && active == 0, "Все 32 writers прошли через Gate");
                Console.WriteLine("WRITERS=32; QUEUED=31; EXCLUSIVE=PASS");
            }
            Check(versions.Contains(Read(target)), "32 конкурентные записи: целая версия");

            using (var firstReady = new ManualResetEventSlim())
            using (var release = new ManualResetEventSlim())
            using (var otherReady = new ManualResetEventSlim())
            using (var sameStarted = new ManualResetEventSlim())
            {
                var first = Run(() => Write(root, "page", "first", temp =>
                {
                    Check(Read(temp) == "first", "Временный файл закрыт и полностью записан");
                    firstReady.Set();
                    Check(release.Wait(10000), "Освобождение первого writer");
                }));
                Check(firstReady.Wait(10000), "Первый writer готов");
                int sameEntered = 0;
                string alias = Path.Combine(root, ".");
                string sameName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "PAGE" : "page";
                Thread sameThread = null;
                var same = Run(() =>
                {
                    sameThread = Thread.CurrentThread;
                    sameStarted.Set();
                    Write(alias, sameName, "same", temp => Interlocked.Exchange(ref sameEntered, 1));
                });
                var other = Run(() => Write(root, "other", "other", temp => otherReady.Set()));
                try
                {
                    Check(sameStarted.Wait(10000), "Второй поток начал вызов writer");
                    Check(SpinWait.SpinUntil(() => Users(target) == 2 &&
                        (sameThread.ThreadState & System.Threading.ThreadState.WaitSleepJoin) != 0, 10000),
                        "Второй writer зарегистрирован и реально заблокирован при удержании первого");
                    Check(otherReady.Wait(10000), "Другие пути независимы");
                    Check(other.Wait(10000) && Read(Path.Combine(root, "other.cshtml")) == "other", "Другой путь опубликован до освобождения первого");
                    Check(Volatile.Read(ref sameEntered) == 0, "Нормализованный путь сериализован");
                }
                finally { release.Set(); }
                Task.WaitAll(first, same, other);
                Check(Read(target) == "same", "Очередь общего пути завершена");
                Check(Users(target) == 0, "Запись реестра удалена после ожидающего writer");
                Console.WriteLine("SECOND_WRITER_BLOCKED=PASS; OTHER_PATH_COMPLETED_WHILE_HELD=PASS");
            }

            string previous = Read(target);
            string foreign = Path.Combine(root, ".foreign.tmp");
            File.WriteAllText(foreign, "чужой файл");
            bool failed = false;
            try { Write(root, "page", "failed", temp => { throw new IOException("Искусственный отказ до публикации"); }); }
            catch (IOException) { failed = true; }
            Check(failed && Read(target) == previous, "Отказ сохраняет предыдущую версию");
            Check(File.ReadAllText(foreign) == "чужой файл", "Чужой временный файл сохранён");
            Check(Directory.GetFiles(root, "*.tmp").Length == 1, "Свой временный файл удалён после отказа");
            failed = false;
            try { Write(root, "absent", "failed", temp => { throw new IOException("Отказ нового файла"); }); }
            catch (IOException) { failed = true; }
            Check(failed && !File.Exists(Path.Combine(root, "absent.cshtml")), "Новый файл не опубликован при отказе");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                failed = false;
                using (var held = new FileStream(target, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    try { Write(root, "page", "denied"); }
                    catch (IOException) { failed = true; }
                }
                Check(failed && Read(target) == previous, "Отказ Windows-публикации сохраняет старое содержимое");

                Write(root, "page", "OLD");
                string residual = null;
                var primary = new IOException("E0.2: первоначальный контролируемый отказ");
                try
                {
                    try
                    {
                        Write(root, "page", "NEW", temp =>
                        {
                            residual = temp;
                            File.SetAttributes(temp, FileAttributes.ReadOnly);
                            ThrowPrimaryFailure(primary);
                        });
                        Check(false, "Двойной отказ должен завершиться исключением");
                    }
                    catch (IOException actual)
                    {
                        Check(ReferenceEquals(actual, primary), "Исходный объект и тип исключения сохранены");
                        Check(actual.Message == "E0.2: первоначальный контролируемый отказ", "Исходное сообщение сохранено");
                        Check(actual.StackTrace.Contains(nameof(ThrowPrimaryFailure)), "Исходный стек сохранён");
                        var cleanup = actual.Data["StoreCMS.RazorPublication.CleanupException"] as UnauthorizedAccessException;
                        Check(cleanup != null && !string.IsNullOrEmpty(cleanup.StackTrace), "Отдельная причина и стек cleanup доступны вызывающему");
                        Check((string)actual.Data["StoreCMS.RazorPublication.TemporaryPath"] == residual && File.Exists(residual),
                            "Диагностика учитывает оставшийся собственный temporary");
                    }
                    Check(Read(target) == "OLD", "OLD сохранён при двойном отказе");
                    lock (Locks) Check(Locks.Count == 0, "Блокировки освобождены при двойном отказе");
                    var next = Run(() => Write(root, "page", "AFTER"));
                    Check(next.Wait(10000) && Read(target) == "AFTER", "Другой поток пишет после двойного отказа");
                    Check(File.Exists(residual) && File.ReadAllText(foreign) == "чужой файл", "Повторная запись не трогает остаточный или чужой temporary");
                    Console.WriteLine("DOUBLE_FAILURE=PASS; PRIMARY_IDENTITY_MESSAGE_STACK=PASS; CLEANUP=UnauthorizedAccessException; RESIDUAL_DIAGNOSED=PASS; LOCKS=0");
                }
                finally
                {
                    if (residual != null && File.Exists(residual))
                    {
                        File.SetAttributes(residual, FileAttributes.Normal);
                        File.Delete(residual);
                    }
                }
            }
            Write(root, "page", versions[0]);
            using (var stop = new CancellationTokenSource())
            using (var readerReady = new ManualResetEventSlim())
            using (var readRequested = new AutoResetEvent(false))
            using (var readAcknowledged = new AutoResetEvent(false))
            {
                int readCount = 0, measuredReads = 0;
                long firstMeasuredRead = 0, lastMeasuredRead = 0;
                var reader = Run(() =>
                {
                    while (!stop.IsCancellationRequested)
                    {
                        bool requested = readRequested.WaitOne(0);
                        long began = Stopwatch.GetTimestamp();
                        Check(versions.Contains(Read(target)), "Чтение только целой версии");
                        long ended = Stopwatch.GetTimestamp();
                        Interlocked.Increment(ref readCount);
                        readerReady.Set();
                        if (requested)
                        {
                            if (measuredReads == 0) firstMeasuredRead = began;
                            lastMeasuredRead = ended;
                            measuredReads++;
                            readAcknowledged.Set();
                        }
                    }
                });
                long updateStart = 0, updateEnd = 0;
                try
                {
                    Check(readerReady.Wait(10000), "Читатель запущен до обновлений");
                    updateStart = Stopwatch.GetTimestamp();
                    for (int i = 0; i < 128; i++) Write(root, "page", versions[i % versions.Length], temp =>
                    {
                        readRequested.Set();
                        Check(readAcknowledged.WaitOne(10000), "Фактическое чтение подтверждено внутри каждого вызова обновления");
                    });
                    updateEnd = Stopwatch.GetTimestamp();
                }
                finally { stop.Cancel(); }
                reader.GetAwaiter().GetResult();
                Check(measuredReads == 128 && updateStart <= firstMeasuredRead && lastMeasuredRead <= updateEnd,
                    "128 полных чтений находятся в измеренном интервале 128 обновлений");
                Check(Read(target) == versions[127 % versions.Length], "Прочитана последняя опубликованная версия");
                Console.WriteLine("READS=" + readCount + "; ACK_READS=" + measuredReads + "; UPDATE_TICKS=" + updateStart + ".." + updateEnd +
                    "; ACK_READ_TICKS=" + firstMeasuredRead + ".." + lastMeasuredRead);
            }
            Check(Directory.GetFiles(root, "*.tmp").SequenceEqual(new[] { foreign }), "Нет оставленных временных файлов");
            lock (Locks) Check(Locks.Count == 0, "Реестр блокировок освобождён после успехов и ошибок");
            Console.WriteLine("PASS: " + checks + " утверждений; " + RuntimeInformation.FrameworkDescription);
        }
        finally { Directory.Delete(root, true); }
    }
}
