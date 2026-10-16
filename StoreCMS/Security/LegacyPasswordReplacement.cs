using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database;
using Treynessen.Database.Context;

namespace Treynessen.Security
{
    // [Комментарий ИИ-агента]: Только локальная операция обслуживания. Метод недоступен через действия MVC или HTTP.
    public static class LegacyPasswordReplacement
    {
        public static void Replace(CMSDatabase db, int userId, string newPassword)
        {
            if (userId <= 0 || !DatabaseInteraction.CorrectPassword(newPassword))
                throw new InvalidOperationException("Invalid administrator ID or new password policy.");
            using (MySqlSecurityInstallation.AcquireLock(db))
            {
                MySqlSecurityInstallation.Validate(db);
                ValidateStorage(db);
                using (var transaction = db.Database.BeginTransaction())
                {
                    var state = db.SecurityInstallation.Single(s => s.ID == 1);
                    if (state.LegacyPasswordResetCompleted)
                        throw new InvalidOperationException("The one-time legacy password replacement is already completed or unavailable.");
                    var user = db.Users.Include(u => u.UserType).SingleOrDefault(u => u.ID == userId);
                    if (user == null || user.UserType.AccessLevel != AccessLevel.VeryHigh
                        || user.Password == null || !Regex.IsMatch(user.Password, "\\A[0-9a-fA-F]{32}\\z"))
                        throw new InvalidOperationException("The selected account is not an eligible legacy administrator.");

                    user.Password = SecurityFunctions.GetPasswordHash(newPassword);
                    user.RequirePasswordChange = false;
                    db.ConnectedUsers.RemoveRange(db.ConnectedUsers.Where(c => c.UserID == userId));
                    state.LegacyPasswordResetCompleted = true;
                    db.SaveChanges();
                    transaction.Commit();
                }
            }
        }

        private static void ValidateStorage(CMSDatabase db)
        {
            // [Комментарий ИИ-агента]: Проверяем реальную схему под той же блокировкой до транзакции и любых изменений данных.
            using (var command = db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE()"
                    + " AND table_name='Users' AND column_name='Password'"
                    + " AND data_type IN ('char','varchar','tinytext','text','mediumtext','longtext')"
                    + " AND character_maximum_length >= 84 AND generation_expression=''";
                if (Convert.ToInt64(command.ExecuteScalar()) != 1)
                    throw new StorageCompatibilityException("Users.Password: требуется записываемый текстовый столбец вместимостью не менее 84 символов. Данные не изменены; тип и длину согласуйте отдельно.");

                command.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema=DATABASE()"
                    + " AND table_name IN ('Users','ConnectedUsers','SecurityInstallation') AND engine='InnoDB' AND table_type='BASE TABLE'";
                if (Convert.ToInt64(command.ExecuteScalar()) != 3)
                    throw new StorageCompatibilityException("Users, ConnectedUsers и SecurityInstallation должны использовать InnoDB для атомарной замены пароля. Данные не изменены; движки автоматически не меняются.");
            }
        }

        public sealed class StorageCompatibilityException : InvalidOperationException
        {
            internal StorageCompatibilityException(string message) : base(message) { }
        }
    }
}
