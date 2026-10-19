# F-IMG-01/02: EF Core/Pomelo и MySQL

100 утверждений на .NET Core 2.2.8/MySQL, включая IR-01. Только существующая локальная test, реквизиты читаются в память из AGENTS.md, без аргументов/файлов с паролем и без чтения Settings/config.json.

Сначала read-only подключение и снимок состава постоянных таблиц/числа строк Images и ProductPages. Затем только TEMPORARY Images/ProductPages в одном удерживаемом соединении, pooling выключен. Они скрывают постоянные таблицы для всех тестовых CMSDatabase этого соединения. В Images есть уникальный ShortPath. Запросы последовательны. В finally - только DROP TEMPORARY TABLE; после него повторно сверяются постоянные таблицы и числа строк. Нет EnsureCreated, миграций, CREATE/DROP DATABASE или изменений постоянных данных.

Собирать и запускать в изолированной копии без пользовательских настроек и Storage:

```powershell
dotnet restore tests/ProductImageSqlRegression --configfile tests/ProductImageLifecycleRegression/NuGet.Config -p:RestoreFallbackFolders="$env:USERPROFILE/.nuget/packages" -p:NuGetAudit=false
dotnet build tests/ProductImageSqlRegression --no-restore
dotnet tests/ProductImageSqlRegression/bin/Debug/netcoreapp2.2/ProductImageSqlRegression.dll <исходный-корень-репозитория>
```

Проверены production-delete/rename с EF tracking на каждой строке наборов 1 / 1,2,3 / 1,3,4 / 1,3,5, повторный DELETE 409, SHA-256 и производные, сохранённые пути/размеры, production-upload с размерами в MySQL. Отказ SaveChanges до SQL сохраняет потенциальную файловую сироту при false; отдельная явно вызванная загрузка не перезаписывает её. Автоматического повтора нет.

IR-01: DiagnosticListener выбрасывает IOException на TransactionCommitted внутри EF после реального commit MySQL. После Dispose аварийного контекста прямой SQL подтверждает одну строку и размеры; оригинал существует, SHA-256 совпадает с опубликованным файлом до исключения, out-признак успеха false, временные файлы очищены. Это внедрение исключения после commit, а не физический обрыв сети. Стенд и журналы исправления: work/ir01-fix235; материалы независимого ревью сохранены.

TEMPORARY-схема без production FK/collation/триггеров. Неоднозначность commit при обрыве сети и конкурентный общий SQL из разных соединений не проверены. Файлы только в %TEMP%/StoreCMS-image-sql-<GUID>, сохраняются для диагностики.
