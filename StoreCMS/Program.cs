using System;
using System.Threading;
using System.Threading.Tasks;
using Treynessen.Hosting;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        using var cancellation = new CancellationTokenSource();
        ConsoleCancelEventHandler cancel = (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };
        Console.CancelKeyPress += cancel;
        try
        {
            using var host = CleanupHost.Create(CmsStartup.CreateHostBuilder(args).Build());
            await CmsStartup.InitializeAsync(host.Services, cancellation.Token);
            // [Комментарий ИИ-агента]: Установка подтверждается до открытия HTTP и запуска фоновой службы.
            await Microsoft.Extensions.Hosting.HostingAbstractionsHostExtensions.RunAsync(host, cancellation.Token);
            return 0;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            return 130;
        }
        catch (CmsStartupException error)
        {
            Console.Error.WriteLine("Запуск StoreCMS остановлен: " + error.Code);
            return 1;
        }
        catch (Exception)
        {
            // [Комментарий ИИ-агента]: Исключение конфигурации может содержать секрет; наружу выдаётся только безопасный код.
            Console.Error.WriteLine("Запуск StoreCMS остановлен: StartupUnavailable");
            return 1;
        }
        finally
        {
            Console.CancelKeyPress -= cancel;
        }
    }
}
