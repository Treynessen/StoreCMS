# StoreCMS — .NET 10 / PostgreSQL 18

Независимый исходный baseline P0 от 16.11.2026. Требуются .NET SDK 10 и PostgreSQL 18; сборка адресуется к проекту без solution:

```powershell
dotnet restore StoreCMS/StoreCMS.csproj --locked-mode --runtime linux-x64
dotnet build StoreCMS/StoreCMS.csproj -c Release --no-restore
```

Существующий `packages.lock.json` закрепляет `linux-x64`; явный RID при restore сохраняет lock без изменений. Команда build выше проверяет сборку проекта, запуск здесь не выполняется.

Перед отдельным запуском создайте приватный `StoreCMS/Settings/config.json` на основе пустого `config.template.json` и настройте заранее подготовленную PostgreSQL-учётную запись. Не сохраняйте credentials, сертификаты, ключи и runtime-данные в Git. Первоначальный запуск изменяет БД и создаёт администратора с обязательной сменой начального пароля; запуск требует отдельной подготовки. На Linux необходимы native-зависимости SkiaSharp и подходящая конфигурация HTTPS.

До первого запуска передайте начальный пароль администратора через переменную окружения `STORECMS_INITIAL_ADMIN_PASSWORD` процессу StoreCMS. Используйте уникальное значение, отвечающее правилам формы смены пароля. После подтверждённой установки уберите переменную из конфигурации запуска и выполните вход с обязательной сменой пароля. Для уже установленной БД переменная не требуется.

Источник: `C:/Users/user/Documents/Codex/StoreCMS/work/p0-standard-baseline-20261103a/source/StoreCMS`. Исходный legacy HEAD: `9546c24d6a6f99b23e4312133605fbba88cfa427`. Сохранены все 339 P0-файлов, включая исправление H-01 проверки PostgreSQL large objects. Добавлены только 25 штатных CSS/JS и пустой шаблон из `output/storecms-p0-linux-x64-20261116a/publish/` старого workspace; сборки этого пакета до H-01 не переносились.

Отчёты в старом workspace: `output/STORECMS_P0_GIT_BASELINE_PLAN_20261116.md`, `output/STORECMS_P0_LARGE_OBJECT_FIX_20261116.md`, `output/STORECMS_P0_GIT_BASELINE_CREATION_20261116.md`.

История, пользовательские данные, tests/tools и стенды не перенесены. Старый репозиторий сохранён; старый blame доступен там. Постоянные регрессионные тесты не входят в этот baseline. Restore/build не подтверждают работу приложения; production deployment и полная release/security-приёмка не выполнены.
