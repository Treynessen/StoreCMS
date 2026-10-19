# F-IMG-01/02: файловые регрессии, 2.3.5

Windows, .NET Core 2.2.8, настоящий ImageSharp beta0006 и production-исходники ImagesManagement/ImageTagHelper. CMSDatabase, журнал и hosting-path заменены двойниками. SQL не выполняется; отдельные контексты на конкурентный запрос.

Собирать в изолированной копии приложения и tests без пользовательских Settings/config.json, Storage, Logs, bin/obj/.vs. Подготовка и журналы текущего прогона: work/image-fixes235/prepare.py и work/image-fixes235/.

```powershell
dotnet restore tests/ProductImageLifecycleRegression --configfile tests/ProductImageLifecycleRegression/NuGet.Config -p:RestoreFallbackFolders="$env:USERPROFILE/.nuget/packages" -p:NuGetAudit=false
dotnet build tests/ProductImageLifecycleRegression --no-restore
dotnet tests/ProductImageLifecycleRegression/bin/Debug/netcoreapp2.2/ProductImageLifecycleRegression.dll
```

132 утверждения: единственный оригинал; каждая строка наборов 1 / 1,2,3 / 1,3,4 / 1,3,5; SHA-256 оригиналов и перенос производных; повтор, устаревшая вкладка, подмена пути/семейства/товара/токена, внешняя замена байтов, возврат к прежнему набору; две загрузки с барьером перед чтением входного потока; остановка в SaveChanges при занятой критической секции; разные товары; размеры/содержимое; отказы временного хранилища, открытия временного файла по ACL, публикации и сохранения метаданных; повтор новым контекстом; очистка/генерация; два реальных дочерних процесса одного товара.

После IR-01 ошибка сохранения метаданных оставляет опубликованный оригинал; явно вызванная следующая загрузка сохраняет его SHA-256 и получает другое имя. Автоматического повтора нет. Прогон исправления: work/ir01-fix235.

ACL-тест через скрытый powershell.exe/.NET Framework меняет только CreateFiles собственной временной папки и снимает правило в finally. Реквизитов БД нет. Синтетические каталоги %TEMP%/StoreCMS-images-<GUID> сохраняются для диагностики; пользовательский Storage не затрагивается.

Межпроцессный тест подтверждает файлы на одном хосте/в одном сеансе ОС, но не общий SQL между процессами. Разные хосты/сеансы Windows, распределённая ФС, внешние записи в Storage, ENOSPC посередине кодирования, аварийное завершение/потеря питания не проверены. Общая атомарность БД/файлов и P2 F-IMG-03/04/05/06 не заявляются.
