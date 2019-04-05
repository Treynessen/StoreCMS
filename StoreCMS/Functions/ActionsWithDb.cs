using Trane.Db.Context;
using Trane.Db.Entities;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Trane.Functions
{
    public static class ActionsWithDb
    {
        public static void SetDefaultUser(CMSContext db)
        {
            if (db.Users.Count() == 0)
            {
                UserType userType = db.UserTypes.FirstOrDefault(t => t.Name.Equals("Admin", StringComparison.CurrentCultureIgnoreCase));
                if (userType == null) userType = db.UserTypes.First();
                db.Users.Add(new User { ID = 1, Login = "admin", Password = "admin", UserType = userType });
                db.SaveChanges();
            }
        }

        // На стороне сервера хранится логин, ключ для входа, ip-адрес, информация о браузере и время последнего действия
        // На стороне клиента, в куках, сохраняем логин и ключ для входа
        public static void AddConnectedUser(CMSContext db, User user, HttpContext context)
        {
            ConnectedUser connectedUser = db.ConnectedUsers.FirstOrDefault(cu => cu.UserName.Equals(user.Login, StringComparison.CurrentCulture));

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
                connectedUser.LoginKey = OtherFunctions.GetRandomKey(7, 13);

                db.ConnectedUsers.Update(connectedUser);
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
                    LoginKey = OtherFunctions.GetRandomKey(7, 13)
                };

                db.ConnectedUsers.Add(connectedUser);
                db.SaveChanges();
            }

            // Использовать вместо userName хэш?
            context.Response.Cookies.Append("userName", connectedUser.UserName);
            context.Response.Cookies.Append("loginKey", connectedUser.LoginKey);
        }

        public static bool AddSimplePage(CMSContext db, SimplePage page)
        {
            if (!Validator.TryValidateObject(page, new ValidationContext(page), null))
                return false;
            string availableSymbols = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890_-";
            page.ID = 1;
            foreach (var p in db.SimplePages)
            {
                if (p.ID != page.ID)
                    break;
                ++page.ID;
            }
            db.Add(page);
            db.SaveChanges();
            return true;
        }
    }
}
