using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Npgsql;

namespace Treynessen.Security
{
    public sealed class SessionExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            for (System.Exception error = context.Exception; error != null; error = error.InnerException)
            {
                var postgres = error as PostgresException;
                if (postgres != null && (postgres.SqlState == PostgresErrorCodes.UniqueViolation
                    || postgres.SqlState == PostgresErrorCodes.DeadlockDetected
                    || postgres.SqlState == PostgresErrorCodes.SerializationFailure))
                {
                    context.Result = new ObjectResult("Конфликт параллельных операций. Повторите действие.") { StatusCode = 409 };
                    context.ExceptionHandled = true;
                    return;
                }
            }
        }
    }
}
