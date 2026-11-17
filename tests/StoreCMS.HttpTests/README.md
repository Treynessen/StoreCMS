# Автономная основа HttpTests — T-01.4.1

Проект net10.0 использует ProjectReference StoreCMS, FrameworkReference Microsoft.AspNetCore.App
и прежние test-only пакеты: Microsoft.NET.Test.Sdk 18.0.1, xunit 2.9.3, runner 3.1.5.
Source-link подключает неизменённые ResourceLedger.cs T-01.3 и NetworkIsolatedTest.cs T-01.1.
PG fixture и installation tests не подключаются. Все cases имеют Category=OfflineHttp.

Проверяются явные строки будущего opt-in (без чтения окружения/credentials), owned temp roots,
CreateNew manifest, marker identity, запрет ссылок, synthetic ledger STOP/ack/readback,
узкий extractor hidden token только login-form, in-memory CA/leaf/chain и настройки HttpClient.
Значения token не передаются в assertion messages или theory arguments.

LiveOptIn распознаёт только enabled=1 и stage=T-01.4.2. Это лишь чистая проверка данных,
а не разрешение пользователем и не запуск live-run: такого кода в проекте нет.
OfflineLedger добавляет общий STOP guard поверх неизменённого ResourceLedger;
RecordSyntheticAbsentReadback является синтетическим входом, не настоящим DB readback
и не доказательством удаления какого-либо объекта PostgreSQL.

OwnedEvidence допускает только отдельного прямого потомка temp с именем StoreCMS-T014-<GUID>,
затем run-<GUID> и случайный marker. Все имена/байты принадлежат тестам. Before-delete scan
проверяет ссылки до рекурсии. Это локальный ownership/marker guard, не OS file-ID attestation
и не защита от враждебной конкурентной замены каталога между проверкой и удалением.
При несовпадении root остаётся; test fixture затем удаляет свою синтетическую область
без следования ссылкам. Соседний sentinel проверяется перед окончательным teardown.
Windows link cases создают junction только между собственными temp directories через
короткий cmd /d /c mklink /J (без окна); Linux-ветвь использует Directory.CreateSymbolicLink.
Это не CMS process; Linux-ветвь до отдельного запуска не считается проверенной.

Extractor намеренно ограничен известной login form: id, method=post без action,
hidden pageID=LoginForm и один hidden __RequestVerificationToken. Meta-token не второй
form-token. Это не HTML5 parser и не проверка криптографической валидности CSRF.
Изменение разметки требует адресного изменения parser/tests либо отдельного решения о parser package.

CA/leaf с SAN localhost/127.0.0.1 создаются в памяти, private keys не экспортируются.
CustomRootTrust содержит только собственную CA; downloads/revocation network выключены
для offline scope. Проверяется chain и отсутствие validation callback/ignore flags;
hostname/SAN validation при соединении и TLS handshake НЕ ПРОВЕРЕНЫ.
HttpClient создаётся без SendAsync/GetAsync; redirects/proxy выключены, timeout ограничены.
NetworkIsolatedTest наблюдает .NET connect/accept/DNS события во время cases; это не OS attestation
и не наблюдение транспорта VSTest/NuGet до теста.

## Восстановление и запуск

Первичное создание нового lock разрешено только без restore production graph:

```text
dotnet restore StoreCMS/StoreCMS.csproj --locked-mode --runtime linux-x64
dotnet restore tests/StoreCMS.HttpTests/StoreCMS.HttpTests.csproj --no-dependencies --use-lock-file
dotnet restore tests/StoreCMS.HttpTests/StoreCMS.HttpTests.csproj --no-dependencies --locked-mode
dotnet build tests/StoreCMS.HttpTests/StoreCMS.HttpTests.csproj -c Release --no-restore -p:UseSharedCompilation=false
dotnet test tests/StoreCMS.HttpTests/StoreCMS.HttpTests.csproj -c Release --no-build --no-restore --list-tests
dotnet test tests/StoreCMS.HttpTests/StoreCMS.HttpTests.csproj -c Release --no-build --no-restore --filter Category=OfflineHttp
```

Production lock/RID и существующие test locks сверять до/после каждой операции;
при изменении STOP без автоматического восстановления. Не передавать graph-wide unlocked
restore или linux-x64 RID Windows test runner. NuGet audit не отключать.
PASS требует ненулевого discovery, двух неизменённых запусков с 0 FAIL/SKIP и exit 0,
совпадающих test names/outcomes и успешного teardown. Discovery не является execution.

В scope нет CMS/HTTP listeners/requests, PostgreSQL, Credential Manager, VM/SSH,
runtime config/secrets, fault injection, browser/UI или Git staging/commit.
T-01.4.2 требует отдельного задания, согласованного private-runtime secret-file exception,
конкретного credential source и нового disposable Windows PG/HTTP scope.