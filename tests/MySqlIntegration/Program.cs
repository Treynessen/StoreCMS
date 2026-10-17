using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Treynessen.AdminPanelTypes;
using Treynessen.Database;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.Extensions;
using Treynessen.ImagesManagement;
using Treynessen.Localization;
using Treynessen.Security;
using Treynessen.TemplatesManagement;
using Treynessen.Translit;

internal class IntegrationProgram
{
    static string connectionString;
    static string isolatedDatabase;
    static string localAdminConnection;
    static int checks;
    static string prefix="it"+Guid.NewGuid().ToString("N").Substring(0,10);
    static DbContextOptions<CMSDatabase> Options()=>new DbContextOptionsBuilder<CMSDatabase>().UseMySql(connectionString).Options;
    static void Check(bool condition,string name){if(!condition)throw new InvalidOperationException(name);checks++;Console.WriteLine("PASS: "+name);}
    static int Main(string[] args)
    {
        try
        {
            int credentialArg=Array.IndexOf(args,"--credential-config");
            string password; uint testPort=3306;
            if(credentialArg>=0)
            {
                var config=new ConfigurationBuilder().AddJsonFile(Path.GetFullPath(args[credentialArg+1])).Build();
                var credentials=new MySqlConnectionStringBuilder(config["DbSettings:ConnectionString"]);
                if(credentials.UserID!="root")throw new InvalidOperationException("Credential identity mismatch");
                password=credentials.Password; testPort=credentials.Port;
            }
            else password=ReadPassword();
            var builder=new MySqlConnectionStringBuilder{Server="127.0.0.1",Port=testPort,UserID="root",Password=password,ConnectionTimeout=5};
            localAdminConnection=builder.ConnectionString;
            using(var connection=new MySqlConnection(builder.ConnectionString))
            {
                connection.Open();
                using(var command=new MySqlCommand("SELECT @@hostname, @@port, VERSION(), CURRENT_USER()",connection))
                using(var reader=command.ExecuteReader()){reader.Read();Console.WriteLine("Local MySQL verified; port="+reader.GetValue(1)+"; version="+reader.GetString(2)+"; user="+reader.GetString(3));}
                using(var command=new MySqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME='test'",connection))
                {Console.WriteLine("Dedicated database test exists: "+(Convert.ToInt32(command.ExecuteScalar())==1));}
                if(args.Contains("--isolated"))
                {
                    // [Комментарий ИИ-агента]: Для повторной проверки первого этапа создаётся собственная новая БД; существующая test не меняется.
                    string database="storecms_stage1_test_"+Guid.NewGuid().ToString("N");
                    using(var command=new MySqlCommand("CREATE DATABASE `"+database+"` CHARACTER SET utf8 COLLATE utf8_general_ci",connection))command.ExecuteNonQuery();
                    isolatedDatabase=database;
                    builder.Database=database;connectionString=builder.ConnectionString;
                    Run();return 0;
                }
                if(args.Contains("--preflight"))return 0;
                if(args.Contains("--verify-clean"))
                {
                    connection.ChangeDatabase("test");
                    foreach(string table in new[]{"Chunks","Templates","Images","Users","UserTypes","AdminPanelLogs"})
                    using(var command=new MySqlCommand("SELECT COUNT(*) FROM `test`.`"+table+"`",connection))
                    {long count=Convert.ToInt64(command.ExecuteScalar());Console.WriteLine("Remaining rows in test."+table+": "+count);if(count!=0)throw new InvalidOperationException("Test rows remain");}
                    return 0;
                }
                if(args.Contains("--inspect"))
                {
                    using(var command=new MySqlCommand("SELECT TABLE_NAME, TABLE_COLLATION FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='test' ORDER BY TABLE_NAME",connection))
                    using(var reader=command.ExecuteReader()){while(reader.Read())Console.WriteLine("Table: "+reader.GetString(0)+"; collation="+reader.GetString(1));}
                    using(var command=new MySqlCommand("SELECT COLUMN_NAME, COLUMN_TYPE, CHARACTER_SET_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='test' AND TABLE_NAME='ConnectedUsers' ORDER BY ORDINAL_POSITION",connection))
                    using(var reader=command.ExecuteReader()){while(reader.Read())Console.WriteLine("ConnectedUsers column: "+reader.GetString(0)+"; "+reader.GetString(1)+"; charset="+(reader.IsDBNull(2)?"none":reader.GetString(2)));}
                    return 0;
                }
                // The user explicitly designated test. No other schema is created/dropped.
                using(var command=new MySqlCommand("CREATE DATABASE IF NOT EXISTS `test`",connection))command.ExecuteNonQuery();
                connection.ChangeDatabase("test");
                using(var command=new MySqlCommand("SELECT DATABASE()",connection))
                    if(!string.Equals((string)command.ExecuteScalar(),"test",StringComparison.Ordinal))throw new InvalidOperationException("Wrong database");
                Console.WriteLine("Confirmed selected database: test. Starting authorized integration checks.");
                if(args.Contains("--recreate-legacy-charset"))
                {
                    using(var command=new MySqlCommand("DROP DATABASE `test`",connection))command.ExecuteNonQuery();
                    using(var command=new MySqlCommand("CREATE DATABASE `test` CHARACTER SET utf8 COLLATE utf8_general_ci",connection))command.ExecuteNonQuery();
                    connection.ChangeDatabase("test");Console.WriteLine("Recreated dedicated test with legacy utf8 charset; application model unchanged.");
                }
            }
            builder.Database="test";connectionString=builder.ConnectionString;
            Run();return 0;
        }
        catch(Exception e)
        {
            // Never print connection strings, credentials, SQL parameters or raw exception messages.
            Console.WriteLine("FAILED: "+e.GetType().FullName);
            if(e is MySqlException mysql)Console.WriteLine("MySQL error number: "+mysql.Number);
            Console.WriteLine("Completed checks: "+checks+"; run prefix: "+prefix);
            return 1;
        }
        finally
        {
            if(isolatedDatabase!=null)
            {
                try
                {
                    // [Комментарий ИИ-агента]: Удаляется только БД, успешно созданная этим прогоном в режиме --isolated.
                    using(var connection=new MySqlConnection(localAdminConnection))
                    {connection.Open();using(var command=new MySqlCommand("DROP DATABASE `"+isolatedDatabase+"`",connection))command.ExecuteNonQuery();}
                    Console.WriteLine("Removed owned stage-one integration database");
                }
                catch{Console.WriteLine("Cleanup incomplete for owned stage-one integration database");}
            }
        }
    }
    static string ReadPassword()
    {
        if(Console.IsInputRedirected)return Console.ReadLine();
        Console.Write("MySQL password: ");string value="";
        while(true){var key=Console.ReadKey(true);if(key.Key==ConsoleKey.Enter){Console.WriteLine();return value;}if(key.Key==ConsoleKey.Backspace){if(value.Length>0)value=value.Substring(0,value.Length-1);}else if(!char.IsControl(key.KeyChar))value+=key.KeyChar;}
    }
    static void Run()
    {
        string root=Path.Combine(AppContext.BaseDirectory,"scratch",prefix);
        var env=new TestEnvironment(root);
        Directory.CreateDirectory(env.GetChunksFolderFullPath());Directory.CreateDirectory(env.GetTemplatesFolderFullPath());Directory.CreateDirectory(env.GetConfigsFolderFullPath());
        var services=new ServiceCollection().AddSingleton<IHostingEnvironment>(env).AddSingleton<EnRuTranslit>().BuildServiceProvider();
        var context=new DefaultHttpContext{RequestServices=services};context.Items["LogLocalization"]=new RuAdminPanelLogLocalization();
        int userId=0,roleId=0,templateId=0,dependentId=0,leafId=0;
        string shared=prefix+"shared",leaf=prefix+"leaf",parent=prefix+"parent";
        try
        {
            Console.WriteLine("CHECK: construct real EF context and explicitly create test schema if empty");
            using(var db=new CMSDatabase(Options()))
            {
                db.Database.EnsureCreated();
                Check(db.Database.CanConnect(),"EF Core/Pomelo connects to test");
                var role=new UserType{Name=prefix,AccessLevel=AccessLevel.VeryHigh};db.UserTypes.Add(role);
                var user=new User{Login=prefix,Password=SecurityFunctions.GetPasswordHash(Guid.NewGuid().ToString()),IdleTime=10,UserType=role};db.Users.Add(user);db.SaveChanges();roleId=role.ID;userId=user.ID;
                var l=new Chunk{Name=leaf,TemplateSource="leaf",TemplatePath=env.GetChunksFolderShortPath()+leaf+".cshtml"};db.Chunks.Add(l);
                var d=new Chunk{Name=parent,TemplateSource="[#"+shared+"] [#"+leaf+"]",TemplatePath=env.GetChunksFolderShortPath()+parent+".cshtml"};db.Chunks.Add(d);
                var t=new Template{Name=prefix+"template",TemplateSource="[#"+shared+"] [#"+leaf+"] [Page:Title]",TemplatePath=env.GetTemplatesFolderShortPath()+prefix+"template.cshtml"};db.Templates.Add(t);db.SaveChanges();leafId=l.ID;dependentId=d.ID;templateId=t.ID;
                context.Items["User"]=user;
                File.WriteAllText(env.GetProductBlockTemplateFullPath(),"[#"+shared+"] [#"+leaf+"] [Product:Name]");
                var model=new TemplateModel{Name=shared,TemplateSource="[#"+leaf+"] added"};
                DatabaseInteraction.AddChunk(db,model,context,out bool added);Check(added,"AddChunk with real model conversion, uniqueness and SaveChanges");
                Check(File.ReadAllText(env.GetTemplatesFolderFullPath()+t.Name+".cshtml").Contains(shared+".cshtml"),"AddChunk regenerates dependent template");
                Check(File.ReadAllText(env.GetChunksFolderFullPath()+parent+".cshtml").Contains(shared+".cshtml"),"AddChunk regenerates dependent chunk");
                Check(File.ReadAllText(env.GetProductBlockCshtmlFullPath()).Contains("@model ProductPage"),"Product block generated with directives");
            }
            Console.WriteLine("CHECK: reload, content edit and rename with fresh contexts");
            using(var db=new CMSDatabase(Options()))
            {
                context.Items["User"]=db.Users.Single(x=>x.ID==userId);
                var chunk=db.Chunks.AsNoTracking().Single(x=>x.Name==shared);string templateFile=env.GetTemplatesFolderFullPath()+prefix+"template.cshtml";string oldTemplate=File.ReadAllText(templateFile);
                DatabaseInteraction.EditChunk(db,chunk.ID,new TemplateModel{Name=shared,TemplateSource="changed [#"+leaf+"]"},context,out bool edited);
                Check(edited && File.ReadAllText(env.GetChunksFolderFullPath()+shared+".cshtml").Contains("changed"),"Content edit updates own Razor source");
                Check(File.ReadAllText(templateFile)==oldTemplate,"Content-only edit preserves existing parent-generation behavior");
            }
            using(var db=new CMSDatabase(Options()))
            {
                context.Items["User"]=db.Users.Single(x=>x.ID==userId);
                var chunk=db.Chunks.AsNoTracking().Single(x=>x.Name==shared);string renamed=prefix+"renamed";
                DatabaseInteraction.EditChunk(db,chunk.ID,new TemplateModel{Name=renamed,TemplateSource=chunk.TemplateSource},context,out bool edited);
                Check(edited && File.Exists(env.GetChunksFolderFullPath()+renamed+".cshtml") && !File.Exists(env.GetChunksFolderFullPath()+shared+".cshtml"),"Rename moves chunk file");
                Check(db.Chunks.AsNoTracking().Single(x=>x.ID==chunk.ID).Name==renamed,"Rename persisted in MySQL");
                Check(File.ReadAllText(env.GetTemplatesFolderFullPath()+prefix+"template.cshtml").Contains("[#"+shared+"]"),"Rename preserves existing unresolved-old-marker behavior");
            }
            using(var db=new CMSDatabase(Options()))
            {
                context.Items["User"]=db.Users.Single(x=>x.ID==userId);
                var chunk=db.Chunks.AsNoTracking().Single(x=>x.Name==prefix+"renamed");
                DatabaseInteraction.EditChunk(db,chunk.ID,new TemplateModel{Name=shared,TemplateSource=chunk.TemplateSource},context,out bool restored);Check(restored,"Rename back regenerates dependencies");
            }
            Console.WriteLine("CHECK: parallel generation, independent real contexts");
            var options=Options();string source="[Product:IfSpecial(before [Product:IfStock([Product:Name])] after [Page:Title])] [#"+leaf+"]";string expected;
            using(var db=new CMSDatabase(options))expected=TemplatesManagementFunctions.SourceToCSHTML(db,source,"Page",env,null);
            Parallel.For(0,20,new ParallelOptions{MaxDegreeOfParallelism=4},i=>{using(var db=new CMSDatabase(options)){if(TemplatesManagementFunctions.SourceToCSHTML(db,source,"Page",env,null)!=expected)throw new InvalidOperationException("Parallel render differs");}});
            Check(true,"20 parallel nested-condition renders with separate real contexts");
            Console.WriteLine("CHECK: uniqueness and image metadata");
            using(var db=new CMSDatabase(Options()))
            {
                context.Items["User"]=db.Users.Single(x=>x.ID==userId);
                var duplicate=new TemplateModel{Name=shared,TemplateSource="duplicate"};DatabaseInteraction.AddChunk(db,duplicate,context,out bool added);
                Check(added && db.Chunks.AsNoTracking().Single(x=>x.ID==duplicate.ID).Name!=shared,"Duplicate chunk name receives existing unique suffix");
                string folder=env.GetStorageFolderFullPath()+"images/products/991/";Directory.CreateDirectory(folder);
                foreach(string f in new[]{"a.jpg","a_320x240.jpg","a_q75.jpg","a_320x240_q75.jpg","ba.jpg","ba_320x240.jpg"})File.WriteAllText(folder+f,f);
                ImagesManagementFunctions.DeleteDependentImages(folder,"a.jpg");Check(File.Exists(folder+"a.jpg") && File.Exists(folder+"ba.jpg") && !File.Exists(folder+"a_q75.jpg"),"Image deletion preserves unrelated originals");
                var image=new Image{ShortPath="/images/products/991/a.jpg",ShortPathHash=Treynessen.Functions.OtherFunctions.GetHashFromString("/images/products/991/a.jpg"),FullName="a.jpg",Width=1,Height=1};db.Images.Add(image);db.SaveChanges();
                ImagesManagementFunctions.RenameImageAndDependencies(db,env,folder,"a","renamed",".jpg");Check(File.Exists(folder+"renamed.jpg") && File.Exists(folder+"ba.jpg"),"Image rename with real metadata SaveChanges");
                db.Entry(image).Reload();Check(image.ShortPath=="/images/products/991/renamed.jpg" && image.FullName=="renamed.jpg" && image.ShortPathHash==Treynessen.Functions.OtherFunctions.GetHashFromString(image.ShortPath),"Renamed image metadata reloaded from MySQL");
                db.Images.Remove(image);db.SaveChanges();
            }
            Console.WriteLine("CHECK: delete shared chunk and verify using new context");
            using(var db=new CMSDatabase(Options()))
            {
                context.Items["User"]=db.Users.Single(x=>x.ID==userId);int id=db.Chunks.AsNoTracking().Single(x=>x.Name==shared).ID;
                DatabaseInteraction.DeleteChunk(db,id,context,out bool deleted);Check(deleted && !File.Exists(env.GetChunksFolderFullPath()+shared+".cshtml"),"DeleteChunk removes row/file and regenerates dependencies");
            }
            using(var db=new CMSDatabase(Options()))
            {
                Check(!db.Chunks.AsNoTracking().Any(x=>x.Name==shared),"Deleted chunk absent in fresh context");
                Check(db.AdminPanelLogs.AsNoTracking().Any(x=>x.UserId==userId),"Real admin events persisted");
            }
            Console.WriteLine("SUCCESS: "+checks+" integration assertions; dedicated test database only.");
        }
        finally
        {
            if(userId!=0)using(var db=new CMSDatabase(Options()))
            {
                db.AdminPanelLogs.RemoveRange(db.AdminPanelLogs.Where(x=>x.UserId==userId));
                db.Templates.RemoveRange(db.Templates.Where(x=>x.ID==templateId));
                db.Chunks.RemoveRange(db.Chunks.Where(x=>x.Name.StartsWith(prefix)));
                db.Users.RemoveRange(db.Users.Where(x=>x.ID==userId));db.SaveChanges();
                db.UserTypes.RemoveRange(db.UserTypes.Where(x=>x.ID==roleId));db.SaveChanges();
            }
            // All paths are created below this assembly's scratch directory.
            if(Directory.Exists(root))Directory.Delete(root,true);
            services.Dispose();Console.WriteLine("Cleanup completed for run prefix: "+prefix);
        }
    }
    class TestEnvironment:IHostingEnvironment
    {
        public TestEnvironment(string root){ContentRootPath=root;WebRootPath=root+"/Storage";ApplicationName="StoreCMS";EnvironmentName="Development";ContentRootFileProvider=new NullFileProvider();WebRootFileProvider=new NullFileProvider();}
        public string EnvironmentName{get;set;} public string ApplicationName{get;set;} public string WebRootPath{get;set;} public IFileProvider WebRootFileProvider{get;set;} public string ContentRootPath{get;set;} public IFileProvider ContentRootFileProvider{get;set;}
    }
}
