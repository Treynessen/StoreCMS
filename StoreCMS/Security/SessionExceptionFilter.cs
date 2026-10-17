using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MySql.Data.MySqlClient;

namespace Treynessen.Security
{
    public sealed class SessionExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            for (System.Exception error = context.Exception; error != null; error = error.InnerException)
            {
                var mysql = error as MySqlException;
                if (mysql != null && (mysql.Number == 1062 || mysql.Number == 1205 || mysql.Number == 1213))
                {
                    context.Result = new ObjectResult("Конфликт параллельных операций. Повторите действие.") { StatusCode = 409 };
                    context.ExceptionHandled = true;
                    return;
                }
            }
        }
    }
}
