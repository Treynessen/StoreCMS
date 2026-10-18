using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Treynessen.AdminPanelTypes;
using Treynessen.Controllers;
using Treynessen.Security;

internal static partial class AntiforgeryRegression
{
    sealed class PrgResultFilter : IActionFilter, IOrderedFilter
    {
        public int Order => int.MaxValue;
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // [Комментарий ИИ-агента]: Только тестовый заголовок подставляет результат после production-защит для проверки PRG без посторонних CRUD.
            string status = context.HttpContext.Request.Headers["X-Test-Prg-Result"];
            if (string.IsNullOrEmpty(status)) return;
            if (status == "throw") throw new InvalidOperationException("Тестовый отказ до успешного результата.");
            context.HttpContext.Response.Headers["X-Test-Preserved"] = "unchanged";
            context.HttpContext.Response.Headers["Location"] = "/fixture-existing-location";
            context.Result = new StatusCodeResult(int.Parse(status));
        }
        public void OnActionExecuted(ActionExecutedContext context) { }
    }

    static async Task PrgRequest(HttpClient client, string label, string path, string token, string page, int expected,
        string destination = null, bool ajax = false, string accept = "text/html", bool tokenHeader = false,
        string result = null, string method = "POST", params string[] fields)
    {
        var data = new Dictionary<string, string>();
        if (page != null) data["pageID"] = page;
        if (token != null && !tokenHeader) data[AdminAntiforgery.FieldName] = token;
        for (int i = 0; i < fields.Length; i += 2) data[fields[i]] = fields[i + 1];
        using (var request = new HttpRequestMessage(new HttpMethod(method), path))
        {
            if (accept != null) request.Headers.TryAddWithoutValidation("Accept", accept);
            if (ajax) request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            if (tokenHeader && token != null) request.Headers.Add(AdminAntiforgery.HeaderName, token);
            if (result != null) request.Headers.Add("X-Test-Prg-Result", result);
            request.Content = new FormUrlEncodedContent(data);
            requests++;
            using (var response = await client.SendAsync(request))
            {
                Check((int)response.StatusCode == expected, label + " expected " + expected + " actual " + (int)response.StatusCode);
                Check(response.Headers.Location?.OriginalString == destination, label + " exact Location");
                if (expected < 500) Check(response.Headers.CacheControl.NoStore, label + " no-store");
                if (result != null && result != "throw")
                    Check(response.Headers.GetValues("X-Test-Preserved").Single() == "unchanged", label + " other headers retained");
                if (expected == 200 || expected == 201)
                    Check((await response.Content.ReadAsByteArrayAsync()).Length == 0, label + " original successful body retained");
                if (expected == 409 && result == null)
                    Check((await response.Content.ReadAsStringAsync()).Contains("user_login_taken")
                        && response.Content.Headers.ContentType.MediaType == "application/json", label + " original JSON error retained");
                if (expected == 303)
                {
                    Check((await response.Content.ReadAsByteArrayAsync()).Length == 0, label + " 303 has no unexpected body");
                    Check(destination.StartsWith("/") && !destination.StartsWith("//") && !destination.Contains("VerificationToken"), label + " local safe destination");
                }
            }
        }
    }

    static async Task NativePrgFlows(HttpClient client, CookieContainer jar)
    {
        int startChecks = checks, startRequests = serverRequests;
        var map = typeof(AdminPanelController).GetMethod("GetNativePostPage", BindingFlags.Static | BindingFlags.NonPublic);
        var supports = typeof(AdminPanelController).GetMethod("SupportsAdminPanelMutation", BindingFlags.Static | BindingFlags.NonPublic);
        var expected = new Dictionary<AdminPanelPages, AdminPanelPages>();
        Action<AdminPanelPages, AdminPanelPages[]> group = (target, pages) => { foreach (var page in pages) expected.Add(page, target); };
        group(AdminPanelPages.MainPage, new[]{AdminPanelPages.LoginForm});
        group(AdminPanelPages.Pages, new[]{AdminPanelPages.AddPage,AdminPanelPages.EditPage});
        group(AdminPanelPages.Categories,new[]{AdminPanelPages.AddCategory,AdminPanelPages.EditCategory,AdminPanelPages.AddProduct,AdminPanelPages.EditProduct});
        group(AdminPanelPages.Redirections,new[]{AdminPanelPages.AddRedirection,AdminPanelPages.EditRedirection});
        group(AdminPanelPages.Templates,new[]{AdminPanelPages.AddTemplate,AdminPanelPages.EditTemplate});
        group(AdminPanelPages.Chunks,new[]{AdminPanelPages.AddChunk,AdminPanelPages.EditChunk});
        group(AdminPanelPages.FileManager,new[]{AdminPanelPages.CreateFolder,AdminPanelPages.CreateStyle,AdminPanelPages.CreateScript,AdminPanelPages.EditStyle,AdminPanelPages.EditScript});
        group(AdminPanelPages.Users,new[]{AdminPanelPages.AddUser,AdminPanelPages.EditUser});
        group(AdminPanelPages.UserTypes,new[]{AdminPanelPages.AddUserType,AdminPanelPages.EditUserType});
        group(AdminPanelPages.SynonymsForStrings,new[]{AdminPanelPages.AddSynonymForString,AdminPanelPages.EditSynonymForString});
        group(AdminPanelPages.UserProfile,new[]{AdminPanelPages.EditUserData});
        group(AdminPanelPages.Settings,new[]{AdminPanelPages.EditSettings});
        Check(expected.Count == 26, "all 26 POST selectors except native initial password mapped");
        foreach (AdminPanelPages page in Enum.GetValues(typeof(AdminPanelPages)))
        {
            object actual = map.Invoke(null, new object[]{(AdminPanelPages?)page});
            Check(expected.TryGetValue(page,out var target) ? actual.Equals(target) : actual == null, "explicit GET map " + page);
            Check((bool)supports.Invoke(null,new object[]{(AdminPanelPages?)page,"POST"}) == (expected.ContainsKey(page) || page == AdminPanelPages.ChangeInitialPassword), "map matches supported POST " + page);
        }

        string anonymous = await CurrentToken(client);
        await PrgRequest(client,"no session", "/admin?pageID=9",anonymous,"46",401);
        await PrgRequest(client,"native login", "/admin?pageID=9",anonymous,"54",303,"/admin?pageID=1",fields:new[]{"Login","fixture","Password",password});
        string token = await CurrentToken(client);
        int number = 0;
        foreach (string path in new[]{"/admin","/private/control-room","/audit-base/admin","/audit-base/private/control-room","/private/space%20room"})
        {
            string name = "native_prg_" + (++number);
            string[] role = {"UserTypeModel.Name",name,"UserTypeModel.AccessLevel","5"};
            await PrgRequest(client,"create native " + path,path+"?pageID=9&returnUrl=https%3A%2F%2Fevil.invalid&itemID=999",token,"46",303,path+"?pageID=9",fields:role);
            string before = MutationSnapshot();
            for (int refresh = 0; refresh < 4; refresh++)
                using (var response = await client.GetAsync(path+"?pageID=9"))
                {
                    Check(response.StatusCode == HttpStatusCode.OK, "follow 303 and refresh return GET HTML");
                    Check((await response.Content.ReadAsStringAsync()).Contains(name), "GET lists created native role");
                }
            Check(Scalar("SELECT COUNT(*) FROM UserTypes WHERE Name='"+name+"'") == "1", "one role after repeated final GET");
            Check(before == MutationSnapshot(),"final GET refresh does not mutate business data");
        }
        string[] valid = {"UserTypeModel.Name","native_prg_ajax","UserTypeModel.AccessLevel","5"};
        await PrgRequest(client,"AJAX accepts HTML", "/admin?pageID=9",token,"46",201,ajax:true,fields:valid);
        await PrgRequest(client,"CSRF header alone is native", "/admin?pageID=9",token,"46",303,"/admin?pageID=9",tokenHeader:true,fields:valid);
        await PrgRequest(client,"no token", "/admin?pageID=9",null,"46",400,fields:valid);
        await PrgRequest(client,"bad token", "/admin?pageID=9","broken","46",400,fields:valid);
        await PrgRequest(client,"AJAX marker cannot replace CSRF", "/admin?pageID=9",null,"46",400,ajax:true,fields:valid);
        string beforeErrors = MutationSnapshot();
        await PrgRequest(client,"invalid role", "/admin?pageID=9",token,"46",422,fields:new[]{"UserTypeModel.Name","bad","UserTypeModel.AccessLevel","0"});
        await PrgRequest(client,"selector body wins rejection", "/admin?pageID=46",token,"9",400,fields:valid);
        await PrgRequest(client,"blank selector rejects", "/admin?pageID=46",token,"",400,fields:valid);
        Check(beforeErrors == MutationSnapshot(), "validation and selector failures preserve business snapshot");
        await PrgRequest(client,"query fallback destination", "/admin?pageID=46",token,null,303,"/admin?pageID=9",fields:valid);
        Sql("UPDATE UserTypes SET AccessLevel=1 WHERE ID=1");
        await PrgRequest(client,"insufficient rights", "/admin?pageID=9",token,"46",403,fields:valid);
        Sql("UPDATE UserTypes SET AccessLevel=5 WHERE ID=1");
        await PrgRequest(client,"native user creation", "/admin?pageID=10",token,"42",303,"/admin?pageID=8",fields:new[]{"UserModel.Login","prgfixture","UserModel.NewPassword",password,"UserModel.UserTypeId","1"});
        await PrgRequest(client,"user conflict stays JSON", "/admin?pageID=10",token,"42",409,fields:new[]{"UserModel.Login","prgfixture","UserModel.NewPassword",password,"UserModel.UserTypeId","1"});
        string id = Scalar("SELECT ID FROM UserTypes WHERE Name='native_prg_1'");
        await PrgRequest(client,"native role edit", "/admin?pageID=42",token,"47",303,"/admin?pageID=9",fields:new[]{"itemID",id,"UserTypeModel.Name","native_prg_edited","UserTypeModel.AccessLevel","4"});
        await PrgRequest(client,"profile no password", "/admin?pageID=46",token,"52",303,"/admin?pageID=11",fields:new[]{"UserModel.ID","1","UserModel.Login","fixture","UserModel.CurrentPassword",password,"UserModel.IdleTime","10"});

        // [Комментарий ИИ-агента]: Результаты подставляются только внутри тестового host после защит; это не доказательство успешного CRUD всех 26 операций.
        foreach (var pair in expected)
            await PrgRequest(client,"mapped pipeline "+pair.Key,"/audit-base/private/control-room?pageID=999",token,((int)pair.Key).ToString(),303,"/audit-base/private/control-room?pageID="+(int)pair.Value,result:"201");
        foreach (string status in new[]{"400","401","403","404","409","422","500","503"})
            await PrgRequest(client,"error unchanged "+status,"/admin",token,"46",int.Parse(status),"/fixture-existing-location",result:status);
        foreach (string accept in new[]{"*/*","application/json","text/html;q=0", "not a media type",null})
            await PrgRequest(client,"non HTML Accept "+accept,"/admin",token,"46",201,"/fixture-existing-location",accept:accept,result:"201");
        await PrgRequest(client,"qualified HTML", "/admin",token,"46",303,"/admin?pageID=9",accept:"application/json, text/html;q=0.5",result:"200");
        await PrgRequest(client,"AJAX original Location", "/admin",token,"46",201,"/fixture-existing-location",ajax:true,result:"201");
        await PrgRequest(client,"PUT unchanged", "/admin",token,"35",201,"/fixture-existing-location",result:"201",method:"PUT");
        await PrgRequest(client,"DELETE unchanged", "/admin",token,"58",200,"/fixture-existing-location",result:"200",method:"DELETE");
        await PrgRequest(client,"unhandled failure unchanged", "/admin",token,"46",500,result:"throw");
        await Status(client,"DELETE","/admin","58",200);
        await PrgRequest(client,"revoked session", "/admin",token,"46",401,fields:valid);
        jar.GetCookies(client.BaseAddress)[SessionCookies.Name].Expired = true;
        Console.WriteLine("PRG native: assertions="+(checks-startChecks)+"; HTTP requests="+(serverRequests-startRequests)+"; real actions and 26 mapped pipeline results passed");
    }
}
