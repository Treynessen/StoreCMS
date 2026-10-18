using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController
    {
        private void ApplyNativePostRedirect(ActionExecutingContext executing, ActionExecutedContext executed)
        {
            if (!HttpMethods.IsPost(Request.Method) || !Request.HasFormContentType
                || executed.Exception != null || Response.HasStarted
                || !(executed.Result is StatusCodeResult result) || (result.StatusCode != 200 && result.StatusCode != 201))
                return;

            // [Комментарий ИИ-агента]: Отдельный признак транспорта сохраняет AJAX-контракт; CSRF, сессия и права уже проверены независимо от него.
            if (string.Equals(Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
                return;
            bool acceptsHtml = false;
            foreach (string entry in Request.Headers["Accept"].ToString().Split(','))
                if (MediaTypeHeaderValue.TryParse(entry, out var media)
                    && string.Equals(media.MediaType.Value, "text/html", StringComparison.OrdinalIgnoreCase)
                    && media.Quality.GetValueOrDefault(1) > 0)
                    acceptsHtml = true;
            if (!acceptsHtml)
                return;

            executing.ActionArguments.TryGetValue("model", out object model);
            AdminPanelPages? destination = GetNativePostPage((model as Model)?.PageId);
            if (!destination.HasValue)
                return;

            // [Комментарий ИИ-агента]: Назначение выбираем по привязанному селектору; чужие URL, query, идентификаторы и секреты в переход не копируем.
            string location = Request.PathBase.Add(Request.Path).ToUriComponent() + "?pageID=" + (int)destination.Value;
            if (!Url.IsLocalUrl(location))
                return;
            Response.Headers["Location"] = location;
            executed.Result = StatusCode(StatusCodes.Status303SeeOther);
        }

        private static AdminPanelPages? GetNativePostPage(AdminPanelPages? page)
        {
            switch (page)
            {
                case AdminPanelPages.LoginForm: return AdminPanelPages.MainPage;
                case AdminPanelPages.AddPage:
                case AdminPanelPages.EditPage: return AdminPanelPages.Pages;
                case AdminPanelPages.AddCategory:
                case AdminPanelPages.EditCategory:
                case AdminPanelPages.AddProduct:
                case AdminPanelPages.EditProduct: return AdminPanelPages.Categories;
                case AdminPanelPages.AddRedirection:
                case AdminPanelPages.EditRedirection: return AdminPanelPages.Redirections;
                case AdminPanelPages.AddTemplate:
                case AdminPanelPages.EditTemplate: return AdminPanelPages.Templates;
                case AdminPanelPages.AddChunk:
                case AdminPanelPages.EditChunk: return AdminPanelPages.Chunks;
                case AdminPanelPages.CreateFolder:
                case AdminPanelPages.CreateStyle:
                case AdminPanelPages.CreateScript:
                case AdminPanelPages.EditStyle:
                case AdminPanelPages.EditScript: return AdminPanelPages.FileManager;
                case AdminPanelPages.AddUser:
                case AdminPanelPages.EditUser: return AdminPanelPages.Users;
                case AdminPanelPages.AddUserType:
                case AdminPanelPages.EditUserType: return AdminPanelPages.UserTypes;
                case AdminPanelPages.AddSynonymForString:
                case AdminPanelPages.EditSynonymForString: return AdminPanelPages.SynonymsForStrings;
                case AdminPanelPages.EditUserData: return AdminPanelPages.UserProfile;
                case AdminPanelPages.EditSettings: return AdminPanelPages.Settings;
                default: return null;
            }
        }
    }
}
