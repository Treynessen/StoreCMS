using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Security
{
    // [Комментарий ИИ-агента]: Механизм для MySQL. Схема существующей установки автоматически не обновляется.
    public static class MySqlSecurityInstallation
    {
        public const int SchemaVersion = 1;

        public static void Initialize(CMSDatabase db)
        {
            using (AcquireInitializationLock(db))
            {
                DbConnection connection = db.Database.GetDbConnection();
                long tableCount = Convert.ToInt64(Scalar(connection,
                    "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema=DATABASE()"));
                if (tableCount != 0)
                {
                    Validate(db);
                    if (!db.SecurityInstallation.AsNoTracking().Single(s => s.ID == 1).LegacyPasswordResetCompleted)
                        throw new InvalidOperationException("Complete the one-time legacy password replacement before starting StoreCMS.");
                    return;
                }

                // [Комментарий ИИ-агента]: При прерывании DDL следующий запуск отклонит непустую БД без маркера установки.
                db.Database.EnsureCreated();
                using (var transaction = db.Database.BeginTransaction())
                {
                    var role = new UserType { Name = "Admin", AccessLevel = AccessLevel.VeryHigh };
                    db.Users.Add(new User
                    {
                        Login = "admin", Password = SecurityFunctions.GetPasswordHash("admin"),
                        UserType = role, IdleTime = 10, RequirePasswordChange = true
                    });
                    db.SecurityInstallation.Add(new SecurityInstallation
                    {
                        ID = 1, SchemaVersion = SchemaVersion, InitializedUtc = DateTime.UtcNow,
                        LegacyPasswordResetCompleted = true
                    });
                    db.SaveChanges();
                    transaction.Commit();
                }
            }
        }

        public static void Validate(CMSDatabase db)
        {
            var connection = db.Database.GetDbConnection();
            if (Convert.ToInt64(Scalar(connection,
                "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema=DATABASE() AND table_name='SecurityInstallation'")) != 1
                || Convert.ToInt64(Scalar(connection,
                "SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='Users' AND column_name='RequirePasswordChange'")) != 1)
                throw new InvalidOperationException("StoreCMS security schema upgrade is required. Automatic administrator creation is disabled for existing databases.");
            var state = db.SecurityInstallation.AsNoTracking().SingleOrDefault(s => s.ID == 1);
            if (state == null || state.SchemaVersion != SchemaVersion)
                throw new InvalidOperationException("StoreCMS security installation marker is missing or unsupported. Review the database before starting the site.");
        }

        // [Комментарий ИИ-агента]: Возвращаемый объект управляет именованной блокировкой и соединением, если оно открыто здесь.
        public static IDisposable AcquireLock(CMSDatabase db)
        {
            var connection = db.Database.GetDbConnection();
            bool opened = connection.State != ConnectionState.Open;
            if (opened) connection.Open();
            return AcquireLock(connection, connection.Database, opened);
        }

        private static IDisposable AcquireInitializationLock(CMSDatabase db)
        {
            var target = db.Database.GetDbConnection();
            if (target.State == ConnectionState.Open)
                return AcquireLock(db);

            // [Комментарий ИИ-агента]: После первого открытия ADO.NET может скрыть пароль; исходная строка берётся из настроек EF без изменения PersistSecurityInfo.
            string configuredConnection = db.GetService<IDbContextOptions>().Extensions
                .OfType<RelationalOptionsExtension>().Single().ConnectionString;
            var settings = new MySqlConnectionStringBuilder(configuredConnection ?? target.ConnectionString);
            string database = settings.Database;
            if (string.IsNullOrWhiteSpace(database))
                throw new InvalidOperationException("StoreCMS: укажите имя целевой базы MySQL в строке подключения.");
            settings.Database = "";
            settings.Pooling = false;
            var server = new MySqlConnection(settings.ConnectionString);
            IDisposable serverLock = null;
            try
            {
                // [Комментарий ИИ-агента]: Те же реквизиты проверяются без выбора БД; блокировка удерживается до окончания всей инициализации.
                server.Open();
                serverLock = AcquireLock(server, database, false);
                try { target.Open(); }
                catch (MySqlException error) when (error.Number == 1049)
                {
                    // [Комментарий ИИ-агента]: Только Unknown database разрешает создание; остальные ошибки подключения передаются вызывающему коду.
                    try
                    {
                        // [Комментарий ИИ-агента]: Кодировка задаётся только для новой БД: прежний составной индекс не помещается в utf8mb4 на MySQL 8.
                        db.GetService<ILoggerFactory>().CreateLogger("StoreCMS.Security.Installation")
                            .LogInformation("StoreCMS CREATE DATABASE: создание отсутствующей базы MySQL.");
                        using (var command = server.CreateCommand())
                        {
                            command.CommandText = "CREATE DATABASE IF NOT EXISTS `" + database.Replace("`", "``")
                                + "` CHARACTER SET utf8 COLLATE utf8_general_ci";
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (MySqlException creationError) when (creationError.Number == 1044 || creationError.Number == 1142 || creationError.Number == 1227)
                    {
                        throw new InvalidOperationException("StoreCMS: отсутствующую базу MySQL не удалось создать. Учётной записи нужны права CREATE DATABASE для целевой базы.", creationError);
                    }
                    target.Open();
                }
                return new InitializationLock(serverLock, server, target);
            }
            catch
            {
                try { serverLock?.Dispose(); }
                finally { target.Close(); server.Dispose(); }
                throw;
            }
        }

        private static IDisposable AcquireLock(DbConnection connection, string database, bool opened)
        {
            string name;
            using (var sha = SHA256.Create())
                name = "storecms-security:" + BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(database))).Replace("-", "").ToLowerInvariant().Substring(0, 40);
            try
            {
                if (!Equals(Convert.ToString(Scalar(connection, "SELECT GET_LOCK(@name, 60)", name)), "1"))
                    throw new InvalidOperationException("Could not acquire StoreCMS security installation lock.");
                return new LockHandle(connection, name, opened);
            }
            catch
            {
                if (opened) connection.Close();
                throw;
            }
        }

        private sealed class InitializationLock : IDisposable
        {
            private readonly IDisposable serverLock;
            private readonly DbConnection server;
            private readonly DbConnection target;
            public InitializationLock(IDisposable serverLock, DbConnection server, DbConnection target)
            { this.serverLock = serverLock; this.server = server; this.target = target; }
            public void Dispose()
            {
                try { target.Close(); }
                finally
                {
                    try { serverLock.Dispose(); }
                    finally { server.Dispose(); }
                }
            }
        }

        private static object Scalar(DbConnection connection, string sql, string name = null)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.CommandTimeout = 70;
                if (name != null)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@name"; parameter.Value = name;
                    command.Parameters.Add(parameter);
                }
                return command.ExecuteScalar();
            }
        }

        private sealed class LockHandle : IDisposable
        {
            private readonly DbConnection connection;
            private readonly string name;
            private readonly bool opened;
            public LockHandle(DbConnection connection, string name, bool opened)
            { this.connection = connection; this.name = name; this.opened = opened; }
            public void Dispose()
            {
                try { Scalar(connection, "SELECT RELEASE_LOCK(@name)", name); }
                finally { if (opened) connection.Close(); }
            }
        }
    }
}
