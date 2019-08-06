using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Treynessen.Security;
using Treynessen.Localization;
using Treynessen.LogManagement;
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
            ConnectedUser connectedUser = db.ConnectedUsers.FirstOrDefault(cu => cu.UserID == user.ID);

            // Если пользователь уже был залогинен, то обновляем его данные
            // Это сделано, если вдруг пользователь заходит с другого браузера
            // С того же самого браузера, с которого он залогинился, пользователь
            // не попадет в этот блок кода, т.к. его либо сразу пропустит в админку, либо 
            // удалится запись из-за длительного бездействия и тогда его данные нужно будет
            // добавить в else блоке
            if (connectedUser != null)
            {
                connectedUser.UserName = SecurityFunctions.GetMd5Hash(SecurityFunctions.GetMd5Hash(user.Login + SecurityFunctions.GetRandomKey(10, 20)));
                connectedUser.LoginKey = SecurityFunctions.GetRandomKey(15, 30);
                connectedUser.LastActionTime = DateTime.Now;
                connectedUser.UserAgent = context.Request.Headers["User-Agent"];
                connectedUser.IPAdress = context.Connection.RemoteIpAddress.ToString();
                db.SaveChanges();
            }

            else
            {
                connectedUser = new ConnectedUser
                {
                    UserName = SecurityFunctions.GetMd5Hash(SecurityFunctions.GetMd5Hash(user.Login + SecurityFunctions.GetRandomKey(10, 20))),
                    LoginKey = SecurityFunctions.GetRandomKey(15, 30),
                    IPAdress = context.Connection.RemoteIpAddress.ToString(),
                    LastActionTime = DateTime.Now,
                    UserAgent = context.Request.Headers["User-Agent"],
                    User = user
                };
                db.ConnectedUsers.Add(connectedUser);
                db.SaveChanges();
            }

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.LoggedIn}. IP: {context.Connection.RemoteIpAddress.ToString()}",
                user: user
            );

            // Использовать вместо userName хэш?
            context.Response.Cookies.Append("userName", connectedUser.UserName);
            context.Response.Cookies.Append("loginKey", connectedUser.LoginKey);
        }
    }
}
