# PostgreSQL IntegrationTests — T-01.3

Проект net10.0 использует production ProjectReference, xUnit 2.9.3,
Microsoft.NET.Test.Sdk 18.0.1 и runner 3.1.5. Все 27 тестов имеют Category=Integration.

## Запуск Windows

Нужны .NET SDK/ASP.NET Core 10 и явно разрешённый локальный PostgreSQL 18.
Provisioner должен иметь права создавать и удалять новые роли/БД; текущая реализация
проверяет SUPERUSER, CREATEDB и CREATEROLE, не повышая привилегии.
Установка выполняется под двумя новыми ограниченными LOGIN-ролями с/без CREATEDB.

Без STORECMS_T013_ENABLED=1 тесты обнаруживаются как NOT CONFIGURED / SKIP
и fixture не подключается к БД. SKIP не является PostgreSQL PASS.
Неполная/неверная включённая конфигурация даёт FAIL без fallback к настройкам CMS.

Задать только несекретные переменные в текущем процессе PowerShell:

```powershell
$env:STORECMS_T013_ENABLED='1'
$env:STORECMS_T013_PURPOSE='disposable-postgres18'
$env:STORECMS_T013_HOST='127.0.0.1'
$env:STORECMS_T013_PORT='5432'
$env:STORECMS_T013_USER='APPROVED_PROVISIONER'
$env:STORECMS_T013_CREDENTIAL_TARGET='APPROVED_CREDENTIAL_MANAGER_TARGET'
$env:STORECMS_T013_EVIDENCE=Join-Path (Get-Location) 'output/t013-evidence'
```

Пароль хранится в уже согласованной Windows Credential Manager entry,
читается CredRead только в память. Код не создаёт/изменяет entries, не читает
config.json CMS и не принимает готовую произвольную целевую БД.
Пароли новых ролей случайны и существуют только в памяти fixture.

До первого restore статически сверять оба lock-файла. Test lock содержит net10.0
и net10.0/linux-x64; production lock не изменяется. Использовать только:

```powershell
dotnet restore tests/StoreCMS.IntegrationTests/StoreCMS.IntegrationTests.csproj --locked-mode --runtime linux-x64
dotnet build tests/StoreCMS.IntegrationTests/StoreCMS.IntegrationTests.csproj -c Release --no-restore
dotnet test tests/StoreCMS.IntegrationTests/StoreCMS.IntegrationTests.csproj -c Release --no-build --no-restore --filter Category=Integration
```

Restore RID обеспечивает существующий production lock contract. Build/test без
RuntimeIdentifier используют обычный net10.0 managed testhost на Windows.
После restore/build/test отдельно сверять raw SHA-256 production lock и Git diff.
Не применять unlocked restore всего графа или force-evaluate при ошибке.

## Границы и очистка

Каждый процесс runner получает новый RunId, криптографически случайные имена
ролей/БД и отдельные БД на case. Имена не используются для поиска/принятия
старых ресурсов. Коллекция выполняется последовательно; синхронизация двух
инициаторов управляется TaskCompletionSource и реальным advisory lock.

До DDL проверяются loopback endpoint, maintenance postgres, current/session user,
PG18 и права provisioner. Для безопасности SQL с паролями проверяется отсутствие
preload hooks/pgaudit; журналирование выключается только в собственных коротких
административных сессиях. Cluster config/service/HBA и чужие sessions не меняются.
Непроверенные logging/privileges приводят к отказу, а не ослаблению guards.

Cleanup разрешён только при успехе всех cases, подтверждении реестра текущего
запуска, OID/owner, завершённого case, совпадении снимка каталогов/таблиц/sequences/LO,
отсутствии подключений и зависимостей ролей. Данные снимка хешируются в памяти,
содержимое и password hashes не записываются в manifest. Перед DROP повторно
проверяется identity; DROP FORCE, DROP OWNED CASCADE и завершение чужих sessions
не применяются. Manifest содержит только имена/OID/owner/run/counts, без секретов.

При ошибке case, неизвестном исходе операции, непроверенной identity или
изменении снимка мутации текущего запуска и автоматический cleanup прекращаются.
Ресурсы остаются для отдельного разбора. Следующий запуск создаёт новые цели
и никогда автоматически не очищает сохранённые предыдущие объекты.
Вывод тестов и TRX не должен содержать credentials или connection strings.

Тесты вызывают настоящие installation services и transaction backend.
Reflection bridge ограничен internal API; уникальное имя assembly не конфликтует
с существующими Unit/ComponentTests. При изменении internal API мост требует ревизии.

Покрыты Empty/Missing, bootstrap/миграции/receipt, повреждения и сохранность,
H-01 до inspect и между inspect/DDL, отмена до DDL, настоящий FK rollback
bootstrap transaction и повтор, два кооперирующих инициатора установки.
Transaction probe не имитирует fault внутри полного coordinator.
Физическая потеря COMMIT ACK не воспроизводится и не объявляется PASS.
Нет HTTP-host, deployment, внешних DDL после последнего SELECT, проверок
publications/subscriptions или подтверждения production readiness.

## Ubuntu

Исходники не требуют смены путей для локального Linux-run. Нужны отдельное
разрешение и подготовка .NET10/PG18/provisioner на самой Ubuntu. Linux credential
adapter читает пароль из inherited anonymous pipe, handle задаётся несекретным
STORECMS_T013_PASSWORD_PIPE. Перед использованием отдельно проверить наследование
handle через dotnet/VSTest/testhost; unattended Linux provisioning здесь не подтверждён.
Не переносить Windows/сохранённые Ubuntu credentials и не читать старые config.
Windows PASS не является Linux PASS. VM автоматически не запускается.


## Fixture-Fix-01 — автономно проверенная редакция

В этой редакции PostgreSQL-прогона и cleanup не было. Исторический отказ на
объединённом guard не приписывается конкретному полю или autovacuum задним числом.

Cleanup различает IdentityMismatch, SnapshotMismatch и TargetSessionsPresent.
При любом отказе сохраняются phase, target identity и безопасный code; дальнейший
DROP запрещён. Sessions не игнорируются и не завершаются принудительно.

Из pg_class исключены только relpages, reltuples, relallvisible, relallfrozen,
relfrozenxid, relminmxid и ленивые relhasindex/relhasrules/relhastriggers/relhassubclass.
Фактические записи pg_index, pg_trigger, pg_rewrite и pg_inherits сохраняются.
Добавлены pg_attrdef, pg_sequence, pg_default_acl, pg_policy и pg_depend.
Все остальные поля и строки каталогов сохраняются, включая неизвестные новые поля.
Identity/owner/ACL/receipt, таблицы, partition roots, materialized views, sequence
definitions и last_value/log_cnt/is_called, LO metadata/ACL/bytes не исключены.
Foreign tables автоматически не читаются: их структура учитывается каталогами.

Snapshot разбит на компоненты Catalog.<name>, DatabaseIdentityAndAcl, TableData,
SequenceState, LargeObjectContents. HMAC-SHA256 использует случайный ключ только
в памяти конкретной БД и длины записей, устраняющие неоднозначность разделителей.
В manifest пишутся только имена изменившихся компонентов; исходные rows, LO,
password hashes, ключи HMAC и snapshot digests не сериализуются.
Первоначальный baseline допускается захватить один раз; последующее сравнение
его не заменяет. READ ONLY snapshot отдельно проверяет фактическую identity.

STORECMS_T013_EVIDENCE теперь обязателен для включённого fixture до подключения.
Новый RunId reserve-файл создаётся через CreateNew; исторические manifests не
загружаются и не переписываются. Manifest обновляется собственным временным файлом
с flush и публикацией rename; абсолютная crash/power-loss durability не аттестована.

Per-object ledger различает Planned, ExpectedAbsent, CreateStarted,
CreateAcknowledged, CreatedConfirmed, UnknownCreateOutcome и отдельно
DropNotAttempted, DropStarted, DropAcknowledged, AbsentReadbackConfirmed,
UnknownDropOutcome. Before-DDL state сохраняется до отправки команды. Ответ
DDL и независимый catalog readback различаются. При исключении после отправки
DDL — STOP/Preserved и запрет retry; события metadata DDL также фиксируются.
Preserved означает запрет автоматической очистки, а не доказанное существование.
Арифметические counters считаются по acknowledged состояниям; неизвестный исход
определяется per-object ledger и требует отдельного readback.
Missing-positive отслеживает production CREATE, Missing-negative — ExpectedAbsent.
Перезапуск не импортирует ledger старых runs и не очищает их цели.

FixtureOfflineChecks.Run — отдельная чистая автономная проверка JSON/HMAC/ledger
на синтетических данных. Она не является xUnit IntegrationTest, не создаёт fixture,
не читает credentials и не открывает PostgreSQL connection. Её можно проверить
отдельным явным вызовом после build; она не заменяет реальный PostgreSQL-прогон.
Последний Windows PG lifecycle всё ещё имеет FAIL; новые полные прогоны и судьба
сохранённых ресурсов требуют отдельного задания владельца.
