# Адресные проверки паролей

Без БД, конфигурации сайта, host и фоновых служб. Целевая платформа теста и проверяемого helper — netcoreapp2.2; используется настоящий PasswordHasher из Microsoft.AspNetCore.App 2.2. В тест подключён исходный GetPasswordHash.cs; User заменён минимальной заглушкой с полем Password. Проверки не подтверждают поведение EF, bootstrap или HTTP-панели.

```powershell
dotnet restore tests/PasswordSecurityRegression --configfile tests/PasswordSecurityRegression/NuGet.Config -p:RestoreFallbackFolders="$env:USERPROFILE/.nuget/packages"
dotnet run --project tests/PasswordSecurityRegression --no-restore
```

2026-10-15: прошли 24 проверки на runtime 2.2.8. Проверены формат Identity V3, PBKDF2-HMAC-SHA256 и 600000 итераций, разные 128-битные соли для одинакового пароля, правильный/неверный пароль и регистр, null/пустые значения, повреждённые/legacy-хеши (включая hex-строку, декодируемую с первым байтом 1), Unicode, параллельное хеширование/проверка и отказ admin после замены хеша. Набор операций занимает менее секунды на локальной машине; это не нагрузочный замер production.

Поддержка legacy-MD5 намеренно отсутствует по решению владельца. Bootstrap, серверная обязательная смена и однократная установка нового пароля с сохранением ID реализованы после отдельного подтверждения; их проверяет tests/AdminSecurityIntegration. Порядок обновления существующего сайта — docs/ADMIN_SECURITY_UPGRADE.md. Этот адресный набор отдельно не подтверждает работу БД/HTTP.
