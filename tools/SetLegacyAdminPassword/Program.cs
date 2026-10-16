using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Treynessen.Database.Context;
using Treynessen.Security;

internal static class MaintenanceProgram
{
    public static int Main(string[] args)
    {
        // [Комментарий ИИ-агента]: Хост приложения, его конфигурация, логгер и фоновые задачи не создаются.
        if (args.Length != 4 || args[0] != "--connection-file" || args[2] != "--user-id"
            || !int.TryParse(args[3], out int userId) || userId <= 0 || Console.IsInputRedirected)
        {
            Console.Error.WriteLine("Usage (interactive terminal only, site stopped): SetLegacyAdminPassword --connection-file <private-json> --user-id <ID>");
            return 2;
        }
        try
        {
            Console.WriteLine("Stop all site instances before continuing. This operation replaces the selected administrator's legacy password once.");
            Console.Write("New password: "); string password = ReadSecret();
            Console.Write("Repeat password: "); string confirmation = ReadSecret();
            if (!string.Equals(password, confirmation, StringComparison.Ordinal))
            { Console.Error.WriteLine("Passwords do not match; no database changes made."); return 2; }
            var config = new ConfigurationBuilder().AddJsonFile(Path.GetFullPath(args[1])).Build();
            string connection = config["DbSettings:ConnectionString"];
            if (string.IsNullOrWhiteSpace(connection))
            { Console.Error.WriteLine("Missing DbSettings:ConnectionString; no database changes made."); return 2; }
            var options = new DbContextOptionsBuilder<CMSDatabase>().UseMySql(connection).Options;
            using (var db = new CMSDatabase(options))
                LegacyPasswordReplacement.Replace(db, userId, password);
            Console.WriteLine("Password replaced once. Administrator ID and related data preserved; previous sessions removed.");
            return 0;
        }
        catch (LegacyPasswordReplacement.StorageCompatibilityException e)
        {
            // [Комментарий ИИ-агента]: Сообщение проверки схемы содержит только фиксированные имена и требования, без реквизитов и SQL.
            Console.Error.WriteLine(e.Message);
            return 1;
        }
        catch (Exception e)
        {
            // [Комментарий ИИ-агента]: Не выводим сообщения провайдера, SQL, строки подключения, пароли или хеши.
            Console.Error.WriteLine("Operation failed (" + e.GetType().Name + "). Review schema, account eligibility and one-time marker locally.");
            return 1;
        }
    }

    private static string ReadSecret()
    {
        string value = "";
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); return value; }
            if (key.Key == ConsoleKey.Backspace) { if (value.Length > 0) value = value.Substring(0, value.Length - 1); }
            else if (!char.IsControl(key.KeyChar)) value += key.KeyChar;
        }
    }
}
