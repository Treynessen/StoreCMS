using Trane.Db.Context;
using Trane.Db.Entities;
using Trane.ViewModels;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace Trane.Functions
{
    public static class DataChecker
    {
        public static User CheckLoginFormData(CMSContext db, LoginFormData data)
        {
            User user = db.Users.FirstOrDefault(u => u.Login == data.Login);

            if (user == null) return null;
            if (user.Password != data.Password) return null;

            db.Entry(user).Reference(u => u.UserType).Load();
            return user;
        }

        // Сравниваем данные в кукисах и хедерах с данными на сервере. 
        public static User CheckCookiesForLF(CMSContext db, HttpContext context)
        {
            string userName = context.Request.Cookies["userName"];

            if (string.IsNullOrEmpty(userName))
                return null;

            ConnectedUser connectedUser = db.ConnectedUsers.FirstOrDefault(cu => cu.UserName == userName);

            if (connectedUser == null)
                return null;

            // Вынести время бездействия хранения в конфиг
            if (DateTime.Now - connectedUser.LastActionTime > new TimeSpan(0, 5, 0))
            {
                db.ConnectedUsers.Remove(connectedUser);
                db.SaveChanges();
                return null;
            }

            // Проверка ip-адреса
            // ...

            if (connectedUser.LoginKey != context.Request.Cookies["loginKey"])
                return null;

            if (connectedUser.UserAgent != context.Request.Headers["User-Agent"])
                return null;

            connectedUser.LastActionTime = DateTime.Now;
            db.Update(connectedUser);
            db.SaveChanges();

            db.Entry(connectedUser).Reference(cu => cu.User).Load();
            db.Entry(connectedUser.User).Reference(u => u.UserType).Load();

            return connectedUser.User;
        }
    }
}