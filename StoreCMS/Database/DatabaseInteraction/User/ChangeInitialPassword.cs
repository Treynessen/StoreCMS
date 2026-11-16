using System;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.Security;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void ChangeInitialPassword(CMSDatabase db, User user, UserModel model, out int statusCode, Microsoft.AspNetCore.Http.HttpContext context = null)
        {
            if (user == null || !user.RequirePasswordChange)
            { statusCode = 403; return; }
            if (model == null || !CorrectPassword(model.NewPassword)
                || !string.Equals(model.NewPassword, model.ConfirmPassword, StringComparison.Ordinal))
            { statusCode = 422; return; }
            if (!SecurityFunctions.VerifyPassword(user, model.CurrentPassword))
            { statusCode = 403; return; }

            string newHash = SecurityFunctions.GetPasswordHash(model.NewPassword);
            string previousHash = user.Password;
            bool previousRequirement = user.RequirePasswordChange;
            bool committed = false;
            try
            {
                using (var transaction = db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var current = SessionStore.LockUser(db, user.ID);
                    if (current == null || current.Password != user.Password || !current.RequirePasswordChange)
                    { statusCode = 409; return; }
                    if (context != null && !SessionStore.IsCurrent(db, context, current, AdminSessionPolicy.Instance, DateTime.UtcNow))
                    { statusCode = 403; return; }
                    // [Комментарий ИИ-агента]: Проверяем сам секрет по актуальному хешу под блокировкой; разные соли не означают смену пароля.
                    if (SecurityFunctions.VerifyPassword(current, model.NewPassword))
                    { statusCode = 422; return; }
                    user.Password = newHash;
                    user.RequirePasswordChange = false;
                    db.SaveChanges(acceptAllChangesOnSuccess: false);
                    SessionStore.RevokeAll(db, user.ID);
                    transaction.Commit();
                    committed = true;
                    db.ChangeTracker.AcceptAllChanges();
                }
            }
            catch (DbUpdateConcurrencyException)
            { statusCode = 409; return; }
            catch (Exception)
            {
                // [Комментарий ИИ-агента]: Не передаём исключение хранилища с возможными параметрами пароля в HTTP-диагностику.
                statusCode = 503; return;
            }
            finally
            {
                // [Комментарий ИИ-агента]: До подтверждения COMMIT восстанавливаем прежние значения; SaveChanges(false) сохраняет исходные значения EF.
                if (!committed)
                {
                    user.Password = previousHash;
                    user.RequirePasswordChange = previousRequirement;
                }
            }
            statusCode = 200;
        }
    }
}
