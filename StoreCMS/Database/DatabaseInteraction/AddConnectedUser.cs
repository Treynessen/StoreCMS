using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Treynessen.Security;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        // На стороне сервера хранится логин, ключ для входа, ip-адрес, информация о браузере и время последнего действия
        // На стороне клиента, в куках, сохраняем логин и ключ для входа
        public static void AddConnectedUser(CMSDatabase db, User user, HttpContext context)
        {
            ConnectedUser connectedUser = db.ConnectedUsers.FirstOrDefault(cu => cu.UserName.Equals(user.Login, StringComparison.InvariantCulture));

            // Если пользователь уже был залогинен, то обновляем его данные
            // Это сделано, если вдруг пользователь заходит с другого браузера
            // С того же самого браузера, с которого он залогинился, пользователь
            // не попадет в этот блок кода, т.к. его либо сразу пропустит в админку, либо 
            // удалится запись из-за длительного бездействия и тогда его данные нужно будет
            // добавить в else блоке
            if (connectedUser != null)
            {
                // connectedUser.IPAdress = ...
                connectedUser.LastActionTime = DateTime.Now;
                connectedUser.UserAgent = context.Request.Headers["User-Agent"];
                connectedUser.LoginKey = SecurityFunctions.GetRandomKey(7, 13);
                db.SaveChanges();
            }

            else
            {
                connectedUser = new ConnectedUser
                {
                    UserName = user.Login,
                    // IPAdress = ...
                    LastActionTime = DateTime.Now,
                    UserAgent = context.Request.Headers["User-Agent"],
                    User = user,
                    LoginKey = SecurityFunctions.GetRandomKey(7, 13)
                };
                db.ConnectedUsers.Add(connectedUser);
                db.SaveChanges();
            }

            // Использовать вместо userName хэш?
            context.Response.Cookies.Append("userName", connectedUser.UserName);
            context.Response.Cookies.Append("loginKey", connectedUser.LoginKey);
        }
    }
}
