# E0.4: чтение опубликованных Razor-файлов

Адресные проверки `GeneratedRazorFileProvider` на Windows, .NET Core 2.2.8 и .NET 10.0.13. БД, production startup и пользовательский Storage не используются.

## Контракт провайдера

```powershell
dotnet run --project tests/RazorReadSharingRegression -c Release -f net10.0
dotnet run --project tests/RazorReadSharingRegression -c Release -f netcoreapp2.2
```

По 58 проверок: границы Templates/Chunks, исключения imports/ViewStart, другие Razor/Settings/расширения/соседние каталоги, метаданные, отсутствующий файл, каталог, traversal, список каталога, порядок и сохранение сторонних провайдеров, повторная регистрация. Удерживаемый DELETE-дескриптор вызывает sharing violation в штатном reader и допускает чтение целого файла обёрткой. Проверяются идентичность change token и callback для шаблона, чанка и imports; переименование при открытом reader, целая старая версия и чтение новой версии. Создаётся только собственный `StoreCMS-E04-<GUID>` в temp с очисткой в finally.

## Опубликованный HTTP

Изолированные исходники/публикации: `work/e04-razor/hosts`; воспроизводимая подготовка из сохранённого стенда E0.2:

```powershell
./tests/RazorReadSharingRegression/setup-hosts.ps1
dotnet publish work/e04-razor/hosts/modern/Probe.csproj -c Release -o work/e04-razor/hosts/published10 -p:NuGetAudit=false
dotnet publish work/e04-razor/hosts/legacy/Probe.csproj -c Release -o work/e04-razor/hosts/published22 -p:NuGetAudit=false
$env:STORECMS_RAZOR_HOSTS = "$PWD/work/e04-razor/hosts"
$env:STORECMS_RAZOR_RESULTS = "$PWD/work/e04-razor"
python tests/RazorPublicationRegression/check-hosts.py
python tests/RazorPublicationRegression/check-imports.py
python tests/RazorReadSharingRegression/check-http.py
```

Стенд E0.2 необходим как историческая тестовая оснастка; прежние HTML-эталоны берутся из сохранённого E0, а не обновляются по текущему выводу. Нужны локальные пакеты, Python/Pillow, runtime 2.2/10 и свободные loopback-порты 51870/51872/51874/51910/51912. В sandbox Windows операции rename и loopback могут требовать запуска вне ограничений.

2.2: `PostConfigure<RazorViewEngineOptions>`. 10: `AddRazorRuntimeCompilation` и `PostConfigure<MvcRazorRuntimeCompilationOptions>`. Обёртываются существующие PhysicalFileProvider на прежних позициях; коллекции не очищаются, ContentRootFileProvider и WebRootFileProvider не заменяются. Обёртка не владеет исходным provider и не освобождает его. В production добавлены только класс и регистрация 2.2; миграция не выполняется.

Матрица: 12 серий каждого сочетания cold/warm и 1 writer + 32 readers, 4 + 4, 32 + 32 на каждом runtime; общий Barrier, 16 запросов на каждого reader, точное сравнение с полными версиями включая LF, измерение пересечения HTTP-интервалов и три контрольных чтения после серии. Всего 72 серии, 888 записей и 26 112 конкурентных чтений на runtime. Отдельно холодная компиляция и инвалидированное прогретое view читаются при удерживаемом DELETE-дескрипторе. Полные интервалы/хеши и ошибки сохраняются в `sharing-http-results.json`; процессы останавливаются в finally.

Прежний HTTP-набор дополнительно проверяет девять точных HTML-эталонов на обоих runtime, вложенные чанки, создание после miss, изменение/инвалидирование, ошибочный Razor и восстановление, повторные чтения, restart. Отдельный набор imports проверяет снятие закешированной ошибки после изменения штатного `_ViewImports.cshtml`.

Ограничения: успешные нагрузочные серии не доказывают отсутствие всех файловых отказов. Share-флаги не обходят ACL/эксклюзивные дескрипторы/антивирус и не дают общей атомарности БД/ФС или нескольких файлов. Imports/ViewStart и product_block сохраняют штатное чтение; прямые записи вне атомарного writer не охвачены. Runtime Compilation в .NET 10 помечена obsolete: это ограничение платформы, а не её замена в E0.4. Linux **NOT TESTED**.
