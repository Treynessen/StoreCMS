using System.Reflection;
using System.Text.Json;
using Treynessen.AdminPanelTypes;
using Treynessen.Database;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.Extensions;
using Treynessen.TemplatesManagement;

internal class Program
{
    static readonly (string Source,string Skip)[] Cases = {
        ("",null),
        ("@mail [Page:Title] [Page:Name] [Product:Price] [HOUR] [Counter id=1] [Counter id=1]",null),
        ("[#alpha] [#beta] [#alpha]",null),
        ("[#alpha] [#beta]","alpha"),
        ("[Product:IfSpecial(before [Product:IfStock([Product:Name])] after [Page:Title])]",null),
        ("[Product:IfStock([Product:IfSpecial([Product:Name])] [Page:Content])]",null),
        ("[Product:IfSpecial(left <image src=\"/a.jpg\"></image> right [Page:Title])]",null),
        ("[Product:Images{skip: 1}{left_side: <div>}{right_side: </div>}] [ProductList] [PageButtons]",null)
    };
    static CMSDatabase Data()
    {
        var db=new CMSDatabase();
        db.Chunks.AddRange(new[]{new Chunk{ID=1,Name="alpha",TemplateSource="A",TemplatePath="~/Views/Chunks/alpha.cshtml"},new Chunk{ID=2,Name="beta",TemplateSource="B",TemplatePath="~/Views/Chunks/beta.cshtml"}});
        return db;
    }
    static string Render(int i,CMSDatabase db=null)=>TemplatesManagementFunctions.SourceToCSHTML(db??Data(),Cases[i].Source,"Page",new Env("unused"),Cases[i].Skip,"@using Example;","@using Other;");
    static void Assert(bool condition,string message) { if(!condition) throw new Exception(message); }
    static void Main(string[] args)
    {
        if(args.Length==2 && args[0]=="--record") { File.WriteAllText(args[1],JsonSerializer.Serialize(Enumerable.Range(0,Cases.Length).Select(i=>Render(i)).ToArray(),new JsonSerializerOptions{WriteIndented=true})); Console.WriteLine("Recorded 8 outputs from pre-change generator");return; }
        var expected=JsonSerializer.Deserialize<string[]>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Expected.json")));
        for(int i=0;i<Cases.Length;i++) Assert(Render(i)==expected[i],"Output differs from original, case "+i);
        Console.WriteLine("PASS: 8 outputs identical to pre-change generator (nested conditions, markers, chunks, additions)");

        var factory=typeof(TemplatesManagementFunctions).GetMethod("CreateInsertionReplacementsCollection",BindingFlags.NonPublic|BindingFlags.Static);
        var a=(LinkedList<InsertionReplacement>)factory.Invoke(null,new object[]{Data(),"[#alpha]",null,new Env("unused")});
        var b=(LinkedList<InsertionReplacement>)factory.Invoke(null,new object[]{Data(),"[#beta]",null,new Env("unused")});
        Assert(a!=null && b!=null && !ReferenceEquals(a,b),"Shared replacement collection");
        Assert(a.Any(x=>x.Insertion=="[#alpha]") && !a.Any(x=>x.Insertion=="[#beta]"),"First collection contaminated");
        Assert(b.Any(x=>x.Insertion=="[#beta]") && !b.Any(x=>x.Insertion=="[#alpha]"),"Second collection contaminated");
        var ta=a.Single(x=>x.Insertion=="[Page:Title]");var tb=b.Single(x=>x.Insertion=="[Page:Title]");
        Assert(!ReferenceEquals(ta,tb),"Shared replacement objects");ta.Replacement="changed";Assert(tb.Replacement!="changed","Shared mutable replacement");
        Console.WriteLine("PASS: collections and replacement objects independent");
        Parallel.For(0,200,i=>Assert(Render(i%Cases.Length)==expected[i%Cases.Length],"Parallel output differs"));
        Console.WriteLine("PASS: 200 parallel renders with separate in-memory contexts");
        foreach(string op in new[]{"add","rename","delete","content"}) Operation(op);
        Console.WriteLine("PASS: 4 real chunk operations with dependent templates/chunks/product block; sequential query instrumentation");
    }
    static void Operation(string operation)
    {
        string root=Path.Combine(AppContext.BaseDirectory,"scratch",Guid.NewGuid().ToString("N"));var env=new Env(root);
        Directory.CreateDirectory(env.GetChunksFolderFullPath());Directory.CreateDirectory(env.GetTemplatesFolderFullPath());Directory.CreateDirectory(root+"/Settings");
        try
        {
            var db=Data();
            db.Templates.Add(new Template{ID=1,Name="page",TemplateSource="[#shared] [#alpha] [Page:Title]"});
            db.Chunks.Add(new Chunk{ID=3,Name="dependent",TemplateSource="[#shared] [#beta]",TemplatePath="~/Views/Chunks/dependent.cshtml"});
            File.WriteAllText(env.GetProductBlockTemplateFullPath(),"[#shared] [#alpha] [Product:Name]");
            File.WriteAllText(env.GetTemplatesFolderFullPath()+"page.cshtml","sentinel");
            File.WriteAllText(env.GetChunksFolderFullPath()+"dependent.cshtml","sentinel");
            File.WriteAllText(env.GetProductBlockCshtmlFullPath(),"sentinel");
            if(operation!="add") { db.Chunks.Add(new Chunk{ID=4,Name="shared",TemplateSource="old",TemplatePath="~/Views/Chunks/shared.cshtml"});File.WriteAllText(env.GetChunksFolderFullPath()+"shared.cshtml","old"); }
            int caller=Environment.CurrentManagedThreadId;
            db.BeforeQuery=()=>Assert(Environment.CurrentManagedThreadId==caller,"Chunk operation queries dispatched concurrently/on another thread");
            var context=new Microsoft.AspNetCore.Http.HttpContext{RequestServices=env};context.Items["LogLocalization"]=null;bool ok;
            if(operation=="add") DatabaseInteraction.AddChunk(db,new TemplateModel{Name="shared",TemplateSource="[#alpha] new"},context,out ok);
            else if(operation=="delete") DatabaseInteraction.DeleteChunk(db,4,context,out ok);
            else DatabaseInteraction.EditChunk(db,4,new TemplateModel{Name=operation=="rename"?"renamed":"shared",TemplateSource=operation=="rename"?"old":"changed [#alpha]"},context,out ok);
            Assert(ok,"Operation failed: "+operation);
            if(operation=="content")
            {
                Assert(File.ReadAllText(env.GetTemplatesFolderFullPath()+"page.cshtml")=="sentinel","Content-only edit unexpectedly regenerates parents");
                Assert(File.ReadAllText(env.GetChunksFolderFullPath()+"shared.cshtml").Contains("changed"),"Edited chunk not generated");
                return;
            }
            foreach(string file in new[]{env.GetTemplatesFolderFullPath()+"page.cshtml",env.GetChunksFolderFullPath()+"dependent.cshtml",env.GetProductBlockCshtmlFullPath()}) Assert(File.ReadAllText(file)!="sentinel","Dependency not regenerated: "+operation);
            Assert(File.ReadAllText(env.GetTemplatesFolderFullPath()+"page.cshtml").Contains("alpha.cshtml"),"Template marker not rendered");
            Assert(File.ReadAllText(env.GetChunksFolderFullPath()+"dependent.cshtml").Contains("beta.cshtml"),"Dependent chunk marker not rendered");
            Assert(File.ReadAllText(env.GetProductBlockCshtmlFullPath()).StartsWith("@model ProductPage\n@using Treynessen.Functions;"),"Product additions/order changed");
            if(operation=="add") Assert(File.ReadAllText(env.GetTemplatesFolderFullPath()+"page.cshtml").Contains("shared.cshtml"),"New dependency missing");
            if(operation=="delete") Assert(!File.Exists(env.GetChunksFolderFullPath()+"shared.cshtml"),"Deleted chunk remains");
            if(operation=="rename") Assert(File.Exists(env.GetChunksFolderFullPath()+"renamed.cshtml") && !File.Exists(env.GetChunksFolderFullPath()+"shared.cshtml"),"Chunk not renamed");
        }
        finally { Directory.Delete(root,true); }
    }
    class Env : Microsoft.AspNetCore.Hosting.IHostingEnvironment,IServiceProvider
    {
        public string Root{get;} public Env(string root){Root=root;}
        public object GetService(Type t)=>t==typeof(Microsoft.AspNetCore.Hosting.IHostingEnvironment)?this:null;
    }
}
