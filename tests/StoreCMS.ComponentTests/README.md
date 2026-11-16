# StoreCMS.ComponentTests — T-01.2

Постоянные xUnit component-тесты .NET 10: настоящая временная файловая система,
production writer/FileManager/ProductDeletionFiles, native SkiaSharp и Razor.
ProjectReference ведёт на текущий StoreCMS; сохранённые runtime/DLL не используются.

Из корня репозитория:

```powershell
dotnet restore StoreCMS/StoreCMS.csproj --locked-mode --runtime linux-x64
dotnet restore tests/StoreCMS.ComponentTests/StoreCMS.ComponentTests.csproj --no-dependencies --locked-mode
dotnet build tests/StoreCMS.ComponentTests/StoreCMS.ComponentTests.csproj -c Release --no-restore -p:UseSharedCompilation=false
dotnet test tests/StoreCMS.ComponentTests/StoreCMS.ComponentTests.csproj -c Release --no-build --no-restore
dotnet test tests/StoreCMS.UnitTests/StoreCMS.UnitTests.csproj -c Release --no-build --no-restore --filter "Category=Unit"
```

Прямые test-only зависимости: Microsoft.NET.Test.Sdk 18.0.1, xunit 2.9.3,
xunit.runner.visualstudio 3.1.5; остальные — из production graph и отдельного lock.
AssemblyName StoreCMS.InstallationValidator.Tests уже разрешён production visibility;
он отличается от StoreCMS.AtomicBootstrap.Tests существующего UnitTests.

Traits: Category=FileSystem или Native; Area=B03/B04/B09/B10/Razor/SkiaSharp.
Каждый case создаёт собственный GUID/случайный временный root StoreCMS-T012-*.
Параллельность отключена: production HostingEnvironmentExtensions кеширует пути
статически. Оснастка сохраняет значения этих кешей, обнуляет перед case и
восстанавливает после. Очистка проверяет абсолютную границу temp, имя каталога,
отсутствие reparse points и успешное удаление. В тестах нет Thread.Sleep.

Windows FileShare-отказы помечены WindowsFact и на других ОС явно пропускаются.
Отсутствующий native Skia не превращается в SKIP/PASS. Два успешных Windows run
не означают проверку Ubuntu. Исходные пакеты и production lock не изменяются.

Границы: CMS/Kestrel/HTTP-host, БД и VM не запускаются. DefaultHttpContext и DI
используются напрямую. FileManager context не содержит User, поэтому штатный
AddAdminPanelLog возвращается до обращения к БД; SQL-журналирование не проверяется.
B09 проверяет настоящий файловый staging/restore/cleanup; переданный синтетический
failure не является SQL rollback. Настоящие DeletePage/DeleteProduct, COMMIT и
целостность строк/счётчиков требуют T-01.3, HTTP CRUD — T-01.4.

B10 — последовательная публикация: при отказе второго файла source новый, view
прежний; обязательна диагностика PartiallyPublished и явное повторное сохранение.
Атомарность пары не предполагается. Windows writer может отказать при длительном
открытом читателе, сохранив прежний файл; provider отдельно проверяется на чтение
целой старой версии после её переименования и публикации новой.

Razor проверяет build-time discovery существующих views и runtime compilation
синтетических шаблона/чанка, отказ повреждённого Razor и исправление через новый
compiler. Это не проверка автоматической инвалидации кеша в живом MVC host.
SourceToCSHTML вызывается без DB-ссылок на chunks (self chunk пропускается).
Имена и traversal проверяются на границе FileManager/PhysicalFileProvider;
WriteCshtmlContentToFile — низкоуровневый writer доверенного пути, sandbox он не
обещает. Разрешение имён chunks из БД и полный MVC request context здесь не доказаны.

FileManager хранит файлы разрешённых расширений без image decode: повреждённый
JPEG может отображаться в listing. Отказ повреждённых байтов проверяется отдельно
на реальном SkiaImageCodec. ImageHandler с EF metadata требует будущих DB-тестов.

Не добавлены ACL/admin faults, crash/power loss, неизвестный COMMIT, HTTP upload
limits и multi-process races. Git staging/commit и исправления production требуют
отдельного задания.
