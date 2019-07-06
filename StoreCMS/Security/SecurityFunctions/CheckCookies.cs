using System;
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

            ConnectedUser connectedUser = db.ConnectedUsers.FirstOrDefaultAsync(cu => cu.UserName.Equals(userName, StringComparison.InvariantCulture)).Result;

            if (connectedUser == null)
                return null;

            // Вынести время бездействия хранения в конфиг
            // Если вышло время бездействия, то отключаем пользователя
            if (DateTime.Now - connectedUser.LastActionTime > new TimeSpan(10, 0, 0))
            {
                db.ConnectedUsers.Remove(connectedUser);
                db.SaveChanges();
                return null;
            }

            // Проверка ip-адреса
            // ...

            // loginKey - это случайно сгенерированный в методе SecurityFunctions.GetRandomKey ключ 
            string userLoginKey = context.Request.Cookies["loginKey"];

            if (!connectedUser.LoginKey.Equals(context.Request.Cookies["loginKey"], StringComparison.InvariantCulture))
                return null;

            if (!connectedUser.UserAgent.Equals(context.Request.Headers["User-Agent"], StringComparison.InvariantCulture))
                return null;

            connectedUser.LastActionTime = DateTime.Now;
            db.Update(connectedUser);
            db.SaveChanges();

            db.Entry(connectedUser).Reference(cu => cu.User).LoadAsync().Wait();
            db.Entry(connectedUser.User).Reference(u => u.UserType).LoadAsync().Wait();

            db.Entry(connectedUser).State = EntityState.Detached;
            db.Entry(connectedUser.User).State = EntityState.Detached;
            db.Entry(connectedUser.User.UserType).State = EntityState.Detached;

            return connectedUser.User;
        }
    }
}