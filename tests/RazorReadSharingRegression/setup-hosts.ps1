$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path "$PSScriptRoot/../..").Path
$seed = "$repo/work/e02-razor/hosts"
$target = "$repo/work/e04-razor/hosts"
if (-not (Test-Path "$seed/shared/ProbeController.cs")) { throw 'Нужен сохранённый изолированный стенд E0.2.' }
foreach ($name in @('modern', 'legacy', 'shared', 'evidence')) {
    New-Item -ItemType Directory -Force "$target/$name" | Out-Null
    foreach ($entry in Get-ChildItem -LiteralPath "$seed/$name") {
        if ($entry.Name -notin @('bin', 'obj')) {
            Copy-Item -LiteralPath $entry.FullName -Destination "$target/$name" -Recurse -Force
        }
    }
}
$manifest = Get-Content "$seed/source-manifest.json" -Raw | ConvertFrom-Json -AsHashtable
$manifest['TemplatesManagement/GeneratedRazorFileProvider.cs'] = ''
foreach ($relative in @($manifest.Keys)) {
    $destination = "$target/shared/Production/$relative"
    New-Item -ItemType Directory -Force (Split-Path $destination) | Out-Null
    Copy-Item -LiteralPath "$repo/StoreCMS/$relative" -Destination $destination -Force
    $manifest[$relative] = (Get-FileHash -LiteralPath $destination).Hash
}
$manifest | ConvertTo-Json | Set-Content "$target/source-manifest.json"
$legacy = "$target/legacy/Program.cs"
$text = [IO.File]::ReadAllText($legacy).Replace('services.AddMvc();', 'services.AddMvc(); services.PostConfigure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(options => Treynessen.TemplatesManagement.GeneratedRazorFileProvider.WrapPhysicalProviders(options.FileProviders));')
[IO.File]::WriteAllText($legacy, $text)
$modern = "$target/modern/Program.cs"
$text = [IO.File]::ReadAllText($modern)
$start = $text.IndexOf('builder.Services.AddControllersWithViews()')
$end = $text.IndexOf('var app =', $start)
$registration = @'
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.PostConfigure<Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation.MvcRazorRuntimeCompilationOptions>(options =>
    Treynessen.TemplatesManagement.GeneratedRazorFileProvider.WrapPhysicalProviders(options.FileProviders));

'@
[IO.File]::WriteAllText($modern, $text.Substring(0, $start) + $registration + $text.Substring($end))
Copy-Item -LiteralPath "$repo/work/e02-razor/e0-evidence.json" -Destination "$repo/work/e04-razor/e0-evidence.json" -Force
