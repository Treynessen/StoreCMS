using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Security
{
    public static partial class SecurityFunctions
    {
        // Сравниваем данные в кукисах и хедерах с данными на сервере. 
        public static User CheckCookies(CMSDatabase db, HttpContext context)
        {
            string userName = context.Request.Cookies["userName"];

            if (string.IsNullOrEmpty(userName))
                return null;

            ConnectedUser connectedUser = db.ConnectedUsers.FirstOrDefault(cu => cu.UserName.Equals(userName, StringComparison.Ordinal));

            if (connectedUser == null)
                return null;

            db.Entry(connectedUser).Reference(cu => cu.User).Load();

            if ((DateTime.Now - connectedUser.LastActionTime).TotalMinutes > connectedUser.User.IdleTime)
            {
                db.ConnectedUsers.Remove(connectedUser);
                db.SaveChanges();
                return null;
            }

            // Проверка ip-адреса
            // ...

            // loginKey - это случайно сгенерированный в методе SecurityFunctions.GetRandomKey ключ 
            string userLoginKey = context.Request.Cookies["loginKey"];

            if (!connectedUser.LoginKey.Equals(context.Request.Cookies["loginKey"], StringComparison.Ordinal))
                return null;

            if (!connectedUser.UserAgent.Equals(context.Request.Headers["User-Agent"], StringComparison.Ordinal))
                return null;

            connectedUser.LastActionTime = DateTime.Now;
            db.Update(connectedUser);
            db.SaveChanges();

            db.Entry(connectedUser.User).Reference(u => u.UserType).Load();

            return connectedUser.User;
        }
    }
}