# Постоянные автономные тесты StoreCMS — T-01.1

Набор unit/component-тестов .NET 10 использует настоящий production-проект через ProjectReference. AssemblyName `StoreCMS.AtomicBootstrap.Tests` соответствует существующему InternalsVisibleTo; production visibility не меняется.

Прямые test-only пакеты: Microsoft.NET.Test.Sdk **18.0.1**, xunit **2.9.3**, xunit.runner.visualstudio **3.1.5**. Версии и транзитивный граф фиксируются в `StoreCMS.UnitTests/packages.lock.json`. Пакет xunit v2 помечен NuGet как legacy; v3 требует отдельного изменения согласованного списка зависимостей.

## Запуск из корня репозитория

Нужны SDK .NET 10 и доступ к NuGet либо заполненный cache. PostgreSQL, credentials, CMS config, Ubuntu VM и сохранённые стенды не нужны.

```text
dotnet restore StoreCMS/StoreCMS.csproj --locked-mode --runtime linux-x64
dotnet restore tests/StoreCMS.UnitTests/StoreCMS.UnitTests.csproj --no-dependencies --locked-mode
dotnet build tests/StoreCMS.UnitTests/StoreCMS.UnitTests.csproj -c Release --no-restore -p:UseSharedCompilation=false
dotnet test tests/StoreCMS.UnitTests/StoreCMS.UnitTests.csproj -c Release --no-build --no-restore --list-tests
dotnet test tests/StoreCMS.UnitTests/StoreCMS.UnitTests.csproj -c Release --no-build --no-restore --filter "Category=Unit"
```

Production restore сохраняет прежний RID и lock; `--no-dependencies` в test restore не переопределяет production graph. Не передавать `--runtime linux-x64` тестовому runner на Windows: тестовая DLL выполняется текущим runtime без RID. NuGet audit не отключён. Недоступность audit и предупреждения надо фиксировать отдельно от успешной сборки/тестов.

Первый разрешённый restore нового тестового проекта выполняется с `--no-dependencies --use-lock-file` до появления его lock. После создания lock Directory.Build.props включает locked mode по умолчанию; команды выше также требуют его явно. Изменять существующий test lock следует только при намеренном обновлении зависимостей. Production lock/RID не менять для устранения ошибок restore; при конфликте остановиться и согласовать минимальное решение.

## Покрытие и изоляция

- Coordinator: Missing/Empty/trusted schema/Existing, порядок вызовов, CREATEDB denied, busy, повреждённое состояние, исключения этапов и неизвестный COMMIT без автоматического retry/cleanup. Backend — double; SQL не исполняется.
- Evaluator: manifest, target identity, markers/system IDs, renamed login, частичные/противоречивые snapshots и bootstrap invariants. Время синтетическое и фиксированное.
- Guards: матрица всех enum-селекторов плюс null/неизвестный и семь HTTP-методов, forced change, HTTPS до CSRF, invalid model, права, anonymous/CSRF/dependency failure. Это прямые component-вызовы с synthetic cached User и AntiforgeryDouble; Kestrel и session backend не запускаются.
- Password: текущий production helper, IdentityV3 interoperability, правильный/неправильный пароль, salts, malformed/legacy/null/empty/Unicode.

Каждый test case наблюдает Socket ConnectStart/AcceptStart и DNS ResolutionStart через EventListener; событие во время теста означает отказ. Тесты исполняются последовательно, чтобы границы наблюдения не пересекались. Это проверка событий .NET в процессе, а не OS-level socket attestation. Транспорт самого VSTest стартует до test case и не относится к HTTP CMS.

Состояние installation receipt хранится/читается Npgsql backend, а не InstallationData evaluator. Его сопоставление и сохранность в PostgreSQL здесь **не проверяются**: Existing/SchemaReady моделируются double. H-01/LO, настоящий bootstrap/COMMIT, HTTP/antiforgery, CRUD, ФС/Razor/native Skia и Windows/Ubuntu integration остаются последующими этапами. Нельзя называть результат этого набора PG/HTTP PASS.

PASS требует ненулевого обнаружения, исполнения всех обязательных tests, отсутствия skipped/failed и exit 0. `--list-tests` подтверждает только discovery. Не менять production и ожидания ради сокрытия failure; сохранять воспроизводящий case и описывать обнаруженную проблему.
