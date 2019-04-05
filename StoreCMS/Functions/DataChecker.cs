using Trane.Db.Context;
using Trane.Db.Entities;
using Trane.ViewModels;
using Trane.Configurations;
using Trane.Controllers.Models;
using Trane.Db.Entities.TypesForEntities;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace Trane.Functions
{
    public static class DataChecker
    {
        public static bool IsValidLoginFormData(CMSContext db, LoginFormModel data, HttpContext context)
        {
            User user = db.Users.FirstOrDefault(u => u.Login.Equals(data.Login, StringComparison.CurrentCulture));
            if (user == null) return false;
            if (!user.Password.Equals(data.Password, StringComparison.CurrentCulture)) return false;
            ActionsWithDb.AddConnectedUser(db, user, context);
            return true;
        }

        // Сравниваем данные в кукисах и хедерах с данными на сервере. 
        public static User CheckCookies(CMSContext db, HttpContext context)
        {
            string userName = context.Request.Cookies["userName"];

            if (string.IsNullOrEmpty(userName))
                return null;

            ConnectedUser connectedUser = db.ConnectedUsers.FirstOrDefault(cu => cu.UserName.Equals(userName, StringComparison.CurrentCulture));

            if (connectedUser == null)
                return null;

            // Вынести время бездействия хранения в конфиг
            if (DateTime.Now - connectedUser.LastActionTime > new TimeSpan(10, 0, 0))
            {
                db.ConnectedUsers.Remove(connectedUser);
                db.SaveChanges();
                return null;
            }

            // Проверка ip-адреса
            // ...

            if (!connectedUser.LoginKey.Equals(context.Request.Cookies["loginKey"], StringComparison.CurrentCulture))
                return null;

            if (!connectedUser.UserAgent.Equals(context.Request.Headers["User-Agent"], StringComparison.CurrentCulture))
                return null;

            connectedUser.LastActionTime = DateTime.Now;
            db.Update(connectedUser);
            db.SaveChanges();

            db.Entry(connectedUser).Reference(cu => cu.User).Load();
            db.Entry(connectedUser.User).Reference(u => u.UserType).Load();

            return connectedUser.User;
        }

        // Проверка прав пользователя для доступа к страницам админ панели
        public static bool HasAccessTo(AdminPanelPages page, User user, HttpContext context)
        {
            if (user == null)
                return false;
            ClearanceLevelConfiguration config = context.Items["ClearanceLevelConfiguration"] as ClearanceLevelConfiguration;
            switch (page)
            {
                case AdminPanelPages.MainPage:
                    return user.UserType.ClearanceLevel >= config.ShowMainPage;
                case AdminPanelPages.Pages:
                    return user.UserType.ClearanceLevel >= config.ShowPages;
                case AdminPanelPages.AddPage:
                    return user.UserType.ClearanceLevel >= config.AddPage;
                case AdminPanelPages.Settings:
                    return user.UserType.ClearanceLevel >= config.AccessToSettings;
                default:
                    return false;
            }
        }
    }
}