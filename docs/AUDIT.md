# Аудит StoreCMS — 14 октября 2026 года

**Вывод:** движок можно модернизировать постепенно, сохранив сайт, адреса страниц и данные. Полная перепись не является необходимым первым шагом. Однако перед дальнейшим развитием нужно исправить аутентификацию, обработку файлов, конкурентную генерацию шаблонов и управление схемой базы. Семь лет работы показывают работоспособность основных сценариев, но не защищённость от специально подобранных запросов или сбоев.

## Что проверено и границы результата

Проверена текущая рабочая папка C:/Users/user/Documents/Codex/StoreCMS, включая несохранённые в Git изменения. Инвентаризация: 248 файлов C#, 45 Razor-представлений, 21 CSS-файл; C# и Razor вместе — 12 584 строки. Проведены поиск опасных конструкций по исходникам и подробный разбор контроллеров, безопасности, контекста и операций БД, шаблонизатора, файлового менеджера, обработки изображений, журналов и фоновых задач. Локализация и стили проверялись прежде всего как часть структуры и мест вывода данных, без отдельной проверки каждой визуальной детали.

Исходный проект не редактировался. Сборка выполнена в отдельной копии, без запуска приложения, без восстановления пакетов из сети и без подключения к MySQL. Копия содержала исходники и существующий файл разрешённых зависимостей; рабочие секретные настройки не переносились.

**Результаты проверки:**

- Сборка отдельной копии с SDK 9.0.101: успешно, 0 ошибок, 14 предупреждений.
- Предупреждения включают устаревшую целевую платформу, известные уязвимости зависимостей и необработанную фоновую задачу.
- Четыре изолированные проверки воспроизвели дефекты удаления изображений, XML-журналов, повторной записи архива и генерации строки C#.
- Для этих проверок использованы оригинальные функции проекта, подключённые к отдельному небольшому приложению на .NET 9; для журнала подставлены минимальные классы данных. Это подтверждает поведение функций, но не заменяет тестирование всего приложения на исходной платформе.
- Runtime .NET Core 2.2 на компьютере не найден. Приложение с реальной БД, браузером и Nginx не запускалось.
- Миграции, SQL-схема, автоматические тесты и файлы CI в предоставленном дереве не обнаружены. Не исследовались история Git, реальные данные и настройки действующего сервера.
- Предупреждения NuGet получены из существующих метаданных разрешения пакетов. Это не новый полный онлайн-аудит всех транзитивных зависимостей.

В проекте указана платформа netcoreapp2.2; в существующем файле зависимостей разрешены Microsoft.AspNetCore.App и EF Core 2.2.0. Утверждение, что действующий сервер работает именно на runtime 2.2.8, по одним исходникам подтвердить нельзя.

Приоритеты: **P1 — высокий**, исправить в первую очередь; **P2 — средний**, включить в ближайшие этапы; **P3 — сопровождение**. «Подтверждено» означает наличие дефекта в коде или изолированной проверке. «Условный риск» означает, что последствия зависят от конфигурации, данных или окружения.

## Безопасность и зависимости

### 01. P1 — платформа и зависимости устарели

**Подтверждено:** [StoreCMS.csproj:4](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/StoreCMS.csproj:4>). Прямые зависимости: Microsoft.AspNetCore.App; Microsoft.AspNetCore.Razor.Design 2.2.0; Pomelo.EntityFrameworkCore.MySql 2.2.0; SixLabors.ImageSharp 1.0.0-beta0006. В существующем графе также присутствуют EF Core 2.2.0 и MySqlConnector 0.49.2.

.NET Core 2.2 завершил поддержку 23 декабря 2019 года. Целевая платформа для модернизации — .NET 10 LTS с поддержкой до 14 ноября 2028 года; поддержка .NET 8 и 9 заканчивается 10 ноября 2026 года, поэтому они неудобны как конечная цель нового переноса. [Политика Microsoft](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core).

Сборка отдельно пометила ImageSharp beta0006 предупреждениями NU1902/NU1903. Среди опубликованных проблем библиотеки — раскрытие содержимого повторно используемых буферов и сбой GIF-декодера. Эксплуатация конкретного экземпляра сайта не проверялась. Обновить библиотеку до актуальной исправленной стабильной версии с проверкой API и лицензии; минимальные версии из старых advisories не считать сегодняшней рекомендацией. [Буферы ImageSharp](https://github.com/SixLabors/ImageSharp/security/advisories/GHSA-5x7m-6737-26cr), [GIF-декодер](https://github.com/SixLabors/ImageSharp/security/advisories/GHSA-2cmq-823j-5qj8).

### 02. P1 — пароль подключения хранится в отслеживаемом конфигурационном файле

**Подтверждено:** Settings/config.json содержит непустое поле пароля в строке подключения и отслеживается Git. Значение в отчёт не включено; действительность пароля и доступность базы не проверялись.

Вынести секрет из репозитория в переменную окружения или хранилище секретов. Если это действующий пароль, сменить его после переноса настроек. Удаление из текущего файла не удаляет значение из истории Git. Секреты мне предоставлять не нужно.

### 03. P1 — автоматический администратор с предсказуемым паролем

**Устранена регрессия первого запуска, 2026-10-15:** после переноса EnsureCreated вызов Open для отсутствующей БД происходил слишком рано. Инициализация теперь берёт GET_LOCK на отдельном подключении без выбора БД и создаёт целевую БД только после ошибки 1049; создание таблиц/администратора/маркера остаётся под этой блокировкой. Существующие базы не удаляются/не пересоздаются, ошибочные реквизиты/права/недоступность не трактуются как отсутствие БД. Для новой БД выбрана utf8/utf8_general_ci из-за прежнего ограничения индекса, кодировки существующих баз не меняются. 65 интеграционных проверок прошли, включая два процесса на отсутствующей БД и отрицательные случаи доступа. D06; подробности в ADMIN_SECURITY_UPGRADE.md.

**Исправлено в согласованном объёме, 2026-10-15:** по решению владельца admin/admin сохраняется только на новой установке. Program вызывает Security/MySqlSecurityInstallation: проверка пустой схемы под GET_LOCK, затем создание схемы и транзакция роль/пользователь/постоянный маркер SecurityInstallation. EnsureCreated удалён из конструктора контекста. Существующая схема без корректного маркера требует явного обновления; удаление/понижение последнего администратора не восстанавливает стандартный аккаунт. Users.RequirePasswordChange и фильтр Controllers/AdminPanelController/InitialPasswordChange.cs запрещают остальные действия до серверной смены пароля; GET отдаёт специальную форму, прямые изменяющие запросы получают 403, выход разрешён. Форма/редирект используют текущий URL. Конкурентная смена защищена optimistic concurrency по Password. План схемы/однократной процедуры подтверждён владельцем; применялся только на одноразовых тестовых БД. Публичная первичная установка всё ещё начинается с известных реквизитов — это явно выбранное поведение, не устранение риска захвата первого входа. Политики последнего администратора/ролей (пункты 09/29) в этой задаче не менялись.

**Исторический дефект до правки:** SetDeafaultUser в Program.cs создавал администратора с известными реквизитами при отсутствии пользователя VeryHigh, в том числе в существующей базе. Старый метод удалён.

**Проверка:** tests/AdminSecurityIntegration — реальные MySQL/EF/MVC/Razor, четыре параллельных процесса инициализации, параллельные HTTP-хосты, повторные запуски, обязательная смена и работа панели, удаление/понижение без bootstrap, отказ неполной/неподдерживаемой схемы. Результаты и границы в PROGRESS.md; обновление существующего сайта — ADMIN_SECURITY_UPGRADE.md.

### 04. P1 — слабое хеширование паролей

**Уточнение по финальному ревью 2.1, исправлено 2026-10-16:** прежние предположения о коротком Password и нетранзакционных таблицах рабочей БД не подтверждались доступом к production. Добавлены проверки реальной схемы: SQL до первого ALTER требует записываемый текстовый Users.Password >=84 символов; legacy-замена под GET_LOCK повторяет это условие и проверяет InnoDB у Users/ConnectedUsers/SecurityInstallation до изменения данных. Автоматического расширения/преобразования нет; утилита выводит безопасную причину отказа. На новой схеме EF/Pomelo подтверждён longtext. 120 интеграционных проверок прошли, включая несовместимые длины/бинарный тип, три MyISAM-таблицы и полную неизменность синтетических записей при отказах. Рабочая схема не проверялась и не менялась; действия владельца описаны в ADMIN_SECURITY_UPGRADE.md.

**Исправлено, 2026-10-15:** GetPasswordHash переведён на PasswordHasher<User> Identity V3 из ASP.NET Core 2.2: PBKDF2-HMAC-SHA256, 600000 итераций, случайная соль 128 бит. Вход и подтверждение текущего пароля в профиле используют VerifyPassword; legacy-MD5 намеренно не принимается. Полная Identity не внедрялась. MD5 для идентификатора сессии не изменён (отдельная следующая задача). Прошли адресные проверки tests/PasswordSecurityRegression на runtime 2.2.8 без БД/host и интеграционные сценарии на реальном MySQL/HTTP.

**Решение владельца и реализация:** миграция MD5 при входе не нужна; рабочая legacy-соль не запрашивается и не сохраняется. tools/SetLegacyAdminPassword вызывает Security/LegacyPasswordReplacement: скрытый двукратный ввод, явный ID, проверка VeryHigh/legacy-формата/одноразового маркера, GET_LOCK и одна транзакция замены пароля/флага/маркера и удаления старой сессии. ID/профиль/роль/журналы сохранены на legacy-фикстуре ID=42; при двух параллельных попытках проходит ровно одна. Постоянного HTTP endpoint нет. Обновление существующего сайта выполняет владелец при остановленном сайте по ADMIN_SECURITY_UPGRADE.md после резервного копирования; рабочая БД не менялась.

**Исторический дефект до правки:** GetPasswordHash использовал 20 итераций MD5 и общую постоянную соль; одинаковые пароли имели одинаковые дешёвые хеши. Этот парольный алгоритм удалён.

Первоначальная рекомендация миграции при успешном входе заменена явным решением владельца об однократной локальной установке нового пароля. Старый MD5 не перехешируется и не проверяется. Изменение политики обычного отзыва сессий, cookies, CSRF и ограничений входа остаётся за рамками этой правки.

### 05. P1 — слабые сессионные ключи и cookies без защитных флагов

**Исправлено в подэтапе 2.2, 2026-10-16:** SessionToken использует CSPRNG 32 байта и SHA-256; ConnectedUsers хранит только хеш, LastActionUtc и диагностические IP/UA. Явные HttpOnly/Secure/SameSite=Lax/host-only Path=/, HTTPS и no-store панели. D08 заменяет исходное предложение абсолютного ограничения: административная политика только скользящая по IdleTime, активная сессия может работать дольше 8 часов. ISessionPolicy отделён от механизма токенов; покупательские функции не реализованы. IP/UA больше не являются условием доступа. Новый SQL/проверка схемы и инструкция: SESSION_SECURITY_UPGRADE.md. Прошли 83 сессионные и 121 административная проверка, включая реальное 27-часовое продление на fake UTC, SQL/гонки/rollback/HTTPS. Фактическая production-топология и браузер ещё не проверены.

Ниже сохранено историческое наблюдение до 2.2; упомянутый GetRandomKey удалён, а рекомендация абсолютного срока заменена D08.

**Подтверждено:** [Security/SecurityFunctions/GetRandomKey.cs:10](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Security/SecurityFunctions/GetRandomKey.cs:10>), [Database/DatabaseInteraction/AddConnectedUser.cs:59](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Database/DatabaseInteraction/AddConnectedUser.cs:59>). Токены генерируются System.Random; cookies записываются без явных HttpOnly, Secure и SameSite. Ключ сессии хранится в базе в открытом виде.

Перейти на штатную cookie-аутентификацию и Data Protection либо криптографически случайные серверные сессии. Указать флаги явно, определить абсолютный и скользящий срок действия. Привязка к IP и User-Agent не заменяет стойкость токена и может ломать вход при смене сети. Её необходимость стоит пересмотреть.

### 06. P1 — отсутствует проверка CSRF-токенов

**Подготовка 2.3.1, 2026-10-17:** устраняются ложный успех Settings/добавления роли и цепочки 302→HTML 200 при отказе изменяющего запроса. Общие XHR-помощники допускают callback только для контрактного 200/201, отдельно показывают HTTP/сетевой отказ; статусы 401/403/default 400 явные. Это подготовка интерфейса и HTTP-контрактов, **не исправление CSRF**: валидации antiforgery и токенов ещё нет. Детали и фактические проверки — tests/AdminHttpErrorRegression и PROGRESS.md.

**Подтверждено отсутствие защиты:** [Controllers/AdminPanelController/AdminPanel(POST).cs:13](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Controllers/AdminPanelController/AdminPanel(POST).cs:13>), [Controllers/AdminPanelController/AdminPanel(PUT).cs:14](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Controllers/AdminPanelController/AdminPanel(PUT).cs:14>), [Controllers/AdminPanelController/AdminPanel(DELETE).cs:15](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Controllers/AdminPanelController/AdminPanel(DELETE).cs:15>). В коде не найдены antiforgery-фильтры или серверная проверка токена. Даже если токен появился бы в форме автоматически, без проверки это не решает проблему.

Включить проверку для изменяющих запросов и передавать токен из AJAX. Проверить также вход и выход. Практический межсайтовый сценарий зависит от поведения браузера и SameSite; современные браузерные ограничения снижают часть риска, но не заменяют серверную защиту.

### 07. P1 — нет ограничений попыток входа

**Подтверждено в приложении:** [Security/SecurityFunctions/IsValidLoginFormData.cs:14](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Security/SecurityFunctions/IsValidLoginFormData.cs:14>). Каждая попытка немедленно проверяет пароль. Для известного логина неудачный вход также создаёт запись в базе.

Добавить ограничение частоты по аккаунту и источнику, прогрессивную задержку, мониторинг неудачных входов. Учитывать доверенный proxy при определении адреса. Наличие отдельной защиты на Nginx неизвестно; её нужно проверить на сервере, не считать отсутствующей автоматически.

### 08. P1 — небезопасный вывод обычных текстовых полей и журналов

**Подтверждено:** [TemplatesManagement/TemplatesManagementFunctions/TemplateRendering/CreateInsertionReplacementsCollection.cs:35](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/TemplatesManagement/TemplatesManagementFunctions/TemplateRendering/CreateInsertionReplacementsCollection.cs:35>), [PagesManagement/PagesManagementFunctions/GetBreadcrumbsHTML.cs:15](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/PagesManagement/PagesManagementFunctions/GetBreadcrumbsHTML.cs:15>), [Views/AdminPanel/UserActions.cshtml:16](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Views/AdminPanel/UserActions.cshtml:16>). Названия страниц, метаданные и log.Info выводятся как готовый HTML. Названия страниц и типов пользователей не ограничены безопасным текстовым форматом.

Это позволяет сохранённым данным становиться исполняемой разметкой. В текущей конфигурации действия разрешены только максимальному уровню, поэтому сценарий между редактором и администратором зависит от последующего разделения ролей. Использовать HTML-кодирование для текстов; для намеренного HTML-контента — отдельную политику очистки и разрешённых тегов. Вывод журналов всегда кодировать.

### 09. P2 — права на управление ролями не ограничивают назначаемый уровень

**Условный риск:** [Database/DatabaseInteraction/User/EditUser.cs:34](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Database/DatabaseInteraction/User/EditUser.cs:34>), [Database/DatabaseInteraction/UserType/EditUserType.cs:30](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Database/DatabaseInteraction/UserType/EditUserType.cs:30>). Проверяется доступ к действию, но не отношение уровня исполнителя к уровню назначаемой роли.

В предоставленных настройках все действия требуют уровня 5, поэтому уже работающий путь повышения прав низким уровнем не подтверждён. Если разрешить EditUser/EditUserType редактору, он сможет назначать более высокий уровень. Ввести явные политики: какие роли может назначать исполнитель и какие аккаунты может изменять.

### 10. P2 — изменение пароля не отзывает сессию

**Исправлено в подэтапе 2.2, 2026-10-16:** обе HTTP-смены пароля обновляют пароль и отзывают сессии одной транзакцией; после успеха требуется новый вход. Выход удаляет предъявленный токен и идемпотентен. Уточнение F-01/D09 от 2026-10-17: очищаются только legacy-cookies; общая auth-cookie не удаляется, чтобы запоздавший ответ не стирал новый вход. Остаточный отозванный токен доступа не даёт. Проверены replay, профиль без смены пароля, сбой отзыва с rollback пароля/сессии и старый logout против новой сессии. Legacy-замена 2.1 сохраняет атомарность. Ниже — историческое поведение до 2.2.

**Подтверждено:** [Database/DatabaseInteraction/User/EditUserData(In user profile).cs:43](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Database/DatabaseInteraction/User/EditUserData(In user profile).cs:43>). После обновления пароля существующий ConnectedUser сохраняется.

При смене пароля отозвать активные сессии или использовать security stamp. При выходе также удалять cookies: сейчас [Database/DatabaseInteraction/Exit.cs:23](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Database/DatabaseInteraction/Exit.cs:23>) удаляет серверную запись, что делает сессию недействительной, но оставляет cookies в браузере.

## MySQL, целостность и конкурентность

### 11. P1 — создание схемы выполняется в каждом конструкторе DbContext

**Подтверждено:** [Database/Context/CMSDataBase.cs:31](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Database/Context/CMSDataBase.cs:31>). EnsureCreated вызывается при каждом создании контекста. Это добавляет проверки БД на пути запроса и не обновляет существующую схему при изменении модели.

Убрать из конструктора. Управлять схемой отдельным процессом миграций и отдельными правами. Для действующей базы сначала сделать baseline на копии, затем сверить схему. Просто добавить Migrate рядом с EnsureCreated небезопасно: эти подходы не сочетаются как обычная последовательность обновления. [Документация EF Core](https://learn.microsoft.com/en-us/ef/core/managing-schemas/ensure-created).

**Дополнение интеграции 2026-10-15:** на локальной MySQL 8.0.40 с utf8mb4_0900_ai_ci EnsureCreated остановился с ошибкой 1071: составной индекс ConnectedUsers по четырём varchar(255) превышает 3072 байта. Подтверждённая test пересоздана с utf8 (utf8mb3); затем проверки прошли. Модель приложения не менялась. Длины/индексы требуют отдельного решения при модернизации, а не молчаливого переноса в PostgreSQL.

### 12. P2 — создание дополнительных DI-контейнеров и обращение к БД для статики

**Подтверждено:** [Startup.cs:25](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Startup.cs:25>), [Middlewares/SitemapMiddleware.cs:20](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Middlewares/SitemapMiddleware.cs:20>). Регистрации зависимостей вызывают BuildServiceProvider вместо переданного provider. Создаются отдельные контейнеры, нарушаются области жизни и усложняется освобождение объектов.

Кроме того, SitemapMiddleware запрашивает CMSDatabase для любого URL и стоит перед статическими файлами. Даже CSS/изображение проходит создание DbContext и EnsureCreated. Использовать штатный provider, регистрацию DbContext с IServiceProvider и отдельный endpoint для sitemap. Отдачу статики не связывать с доступностью MySQL.

### 13. Общий изменяемый список генератора — исправлен 2026-10-15 (исходный приоритет P1)

**Исходный дефект:** insertionReplacements был общим static LinkedList, очищаемым и наполняемым каждым рендером. Чтение происходило в SourceToCSHTML и рекурсивной обработке условий; ReplaceConditionalExpressions также изменял элементы. Параллельные вызовы могли смешивать замены или получать ошибку перечисления.

**Исправлено:** CreateInsertionReplacementsCollection возвращает новый список с новыми элементами для каждого непустого SourceToCSHTML. Коллекция передаётся в ReplaceConditionalExpressions и ReplaceConditionalExpressionValue, включая все рекурсивные вызовы. Порядок замен, маркеры и алгоритм условий сохранены. [Код генератора](../StoreCMS/TemplatesManagement/TemplatesManagementFunctions/TemplateRendering/).

**Проверено автоматически:** независимость двух коллекций/элементов, 200 параллельных рендеров с разными заглушками контекста, полное совпадение 8 результатов с исходной версией (включая вложенные условия). [Проверки и ограничения](../tests/TemplateRegression/README.md). Razor runtime и параллельная публикация одного и того же файла не проверялись; эта правка не вводит блокировку файлов между запросами.

Дополнительно 2026-10-15: 20 параллельных генераций вложенных условий прошли с отдельными настоящими CMSDatabase/EF Core/Pomelo и MySQL 8.0.40 в выделенной test с utf8. [Интеграционный прогон и ограничения](../tests/MySqlIntegration/README.md).

### 14. Один DbContext в параллельных ветках операций с чанками — исправлено 2026-10-15 (исходный приоритет P1)

**Исходный дефект:** AddChunk/DeleteChunk и ветка переименования EditChunk запускали шаблоны через Task.Run с тем же db, пока текущий поток генерировал чанки/блок товара. Wait стоял в конце и не исключал пересечение. Предварительный ToList не помогал: GetChunks внутри генератора снова обращается к db.

**Исправлено:** Task.Run/Wait удалены из трёх методов; обработка последовательная: шаблоны → зависимые чанки → блок товара. Выборки, условия переименования, skipChunkName, директивы и сохранение данных не менялись. [Операции с чанками](../StoreCMS/Database/DatabaseInteraction/Chunk/).

**Проверено автоматически:** реальные AddChunk/EditChunk/DeleteChunk с заглушками окружения/БД перегенерируют зависимые файлы; инструментирование запросов проверяет отсутствие перехода на параллельную ветку и пересечения запросов. Отдельно проверена правка только содержимого без перегенерации родителей. Дополнительно 2026-10-15 реальные EF Core/Pomelo/MySQL проверены набором MySqlIntegration: добавление, изменение содержимого, переименование и удаление с генерацией зависимостей, уникальность имён и сохранение журналов прошли в test с utf8. Отображение Razor/админские HTTP-сценарии в браузере не проверены; полноценный сайт не запускался. Выбор .NET 10/EF Core/PostgreSQL через Npgsql (D03) не затронут.

### 15. Дефект отбора производных изображений — исправлен 2026-10-15

**Дефект метода подтверждён:** прежний DeleteDependentImages сопоставлял regex с полным путём без привязки к началу имени и без экранирования имени/расширения. В изолированном примере с a.jpg и ba.jpg в одной папке обработка a.jpg удаляла ba.jpg и его производные. Аналогичный отбор был в RenameImageAndDependencies. Первоначальная оценка P1 относилась к возможности потери файлов при таком содержимом каталога, а не к доказанному штатному сценарию каталога товаров.

**Уточнение структуры:** UploadProductImageToServer и DeleteProductImage используют Storage/images/products/{product.ID}/. Внутри папки — оригиналы alias.jpg, alias_2.jpg и далее, а также варианты name_WIDTHxHEIGHT.jpg, name_qQUALITY.jpg и name_WIDTHxHEIGHT_qQUALITY.jpg, создаваемые ImageHandler/ApplySettings.cs. Поэтому конфликт разных товаров a/ba при штатной загрузке не подтверждён: их папки разделены по ID. Общий файловый менеджер также допускает произвольно именованные изображения в доступных каталогах; сам метод должен корректно работать независимо от ограничения товарной загрузки.

**Исправлено:** оба метода сопоставляют Path.GetFileName по шаблону с границами всей строки и Regex.Escape для имени и расширения. DeleteDependentImages сохраняет оригинал; RenameImageAndDependencies переносит оригинал и только его варианты, сохраняя суффиксы. Схема размещения, нумерация и обновление метаданных не изменены. Файлы: [DeleteDependentImages.cs](../StoreCMS/ImagesManagement/ImagesManagementFunctions/DeleteDependentImages.cs), [RenameImageAndDependencies.cs](../StoreCMS/ImagesManagement/ImagesManagementFunctions/RenameImageAndDependencies.cs).

**Проверено:** 16 изолированных регрессионных сценариев в [tests/ImageFileRegression](../tests/ImageFileRegression/README.md): a/ba, реальные alias/alias_N, размеры/качество, соседние оригиналы, неверные суффиксы, regex-символы, разные расширения, пути с/без завершающего разделителя, отсутствующий оригинал и изоляция папок товаров. Подключены оригинальные методы; БД заменена заглушкой. Это проверка файлового поведения, не интеграционный тест MySQL.

Дополнительно 2026-10-15: DeleteDependentImages и RenameImageAndDependencies проверены с реальным EF/MySQL в test; после переименования запись Image повторно загружена из БД и проверены ShortPath/FullName/хеш. Связанные файлы изолированы от рабочего Storage.

### 16. P1 — записи базы и файлы могут расходиться после ошибки

**Подтверждено:** [Database/DatabaseInteraction/Template/EditTemplate.cs:46](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Database/DatabaseInteraction/Template/EditTemplate.cs:46>), [ImagesManagement/ImagesManagementFunctions/UploadProductImageToServer.cs:70](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/ImagesManagement/ImagesManagementFunctions/UploadProductImageToServer.cs:70>), [Database/DatabaseInteraction/Page/DeletePage.cs:51](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Database/DatabaseInteraction/Page/DeletePage.cs:51>).

Шаблон сначала сохраняется в БД, затем записывается на диск. При загрузке изображения запись БД появляется до сохранения файла. При удалении обычной страницы первая фиксация происходит до восстановления связей дочерних страниц. Ошибка посередине оставляет частично выполненную операцию.

Объединить изменения БД в транзакцию. Для файлов применить временную запись и атомарную публикацию, журнал операции/компенсацию и восстановление после сбоя. Транзакция MySQL сама по себе не откатывает File.Delete или File.Move.

### 17. P2 — конфигурационный JSON формируется вручную и перезаписывается целиком

**Подтверждено:** [SettingsManagement/ConfigurationHandler/ChangeConfigFile.cs:10](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/SettingsManagement/ConfigurationHandler/ChangeConfigFile.cs:10>). Некоторые строки вставляются без JSON-экранирования. Кавычка или перенос строки в PaginationStyleName может сделать конфигурацию нечитаемой. Параллельные сохранения теряют изменения; другой запрос может прочитать незавершённый файл.

Использовать JSON-сериализатор, типизированные валидируемые настройки, временный файл и атомарную замену. Сериализовать сохранение или проверять версию настроек. Секреты подключения не переписывать вместе с редактируемыми настройками CMS.

### 18. P1 — файловый логгер может блокировать остальные потоки

**Подтверждено:** [LogManagement/FileLogger.cs:36](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/LogManagement/FileLogger.cs:36>). HttpContext используется без проверки на null; фоновые ошибки не обязаны иметь HTTP-запрос. После WaitOne отсутствует finally для освобождения Mutex.

Ошибка открытия/записи файла может оставить mutex захваченным и остановить следующие записи из других потоков. Использовать штатную инфраструктуру структурированного логирования, устойчивый sink, ротацию и поддержку фоновых событий. Временное исправление — проверка контекста и гарантированное освобождение блокировки.

### 19. P2 — ежедневная задача не защищена от сбоев, повторов и нескольких экземпляров

**Подтверждено устройство:** [TimedHostedService/DailyTask.cs:10](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/TimedHostedService/DailyTask.cs:10>), [TimedHostedService/StartAsync.cs:12](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/TimedHostedService/StartAsync.cs:12>), [TimedHostedService/TransferLogsFromDbToFiles.cs:15](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/TimedHostedService/TransferLogsFromDbToFiles.cs:15>). Timer вызывает синхронную задачу без обработки ошибок. Необработанное исключение callback может завершить процесс; выполнение на нескольких экземплярах не координируется.

**Воспроизведён частный дефект:** [LogManagement/LogManagementFunctions/UserLogsToLogFile.cs:18](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/LogManagement/LogManagementFunctions/UserLogsToLogFile.cs:18>) повторно открывает дневной файл с перезаписью. Второй вызов для того же дня заменяет первую порцию событий. Используется вчерашняя дата от DateTime.Now, переданный currentDay игнорируется, а события не фильтруются по дню.

Перейти на наблюдаемую последовательную фоновую задачу с отменой, retry, блокировкой между экземплярами и идемпотентным архивированием по идентификаторам событий. Удалять данные из БД только после подтверждённой сохранности архива.

### 20. P2 — XML-журнал создаётся без XML-экранирования

**Воспроизведено:** [LogManagement/LogManagementFunctions/UserLogsToLogFile.cs:23](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/LogManagement/LogManagementFunctions/UserLogsToLogFile.cs:23>). Событие A & B создаёт невалидный XML. Название страницы с амперсандом — обычный легитимный источник такой ошибки.

Писать через XmlWriter/XDocument или выбрать структурированный JSON. Читать тем же форматом, а не регулярными выражениями. [Controllers/AdminPanelController/UserProfile/GetUserLog.cs:47](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Controllers/AdminPanelController/UserProfile/GetUserLog.cs:47>) восстанавливает HTML из текста журнала без кодирования, что дополнительно связано с пунктом 08.

### 21. P1 — поиск может разрастаться и выполнять опасные регулярные выражения

**Подтверждено устройство, масштаб зависит от данных:** [Controllers/PageController/SearchPage.cs:47](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Controllers/PageController/SearchPage.cs:47>). Синонимы вставляются в regex без Regex.Escape и без timeout. Символы регулярных выражений меняют смысл или делают выражение невалидным.

Каждый синоним добавляет новые варианты запроса, которые затем объединяются в цепочку Union. Число вариантов может расти экспоненциально; ограничение длины пользовательского поиска не ограничивает число вариантов.

Ввести лимиты вариантов, слов и вычислительного времени, удалять дубли, экранировать литералы. Для значимого каталога оценить индексируемый поиск вместо множества Contains. Сложную нагрузку проверять только на тестовой базе.

### 22. P2 — перенос LINQ-запросов на современный EF требует проверки SQL

**Риск совместимости:** [RequestManagement/RequestHandler/FindPage.cs:20](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/RequestManagement/RequestHandler/FindPage.cs:20>), [Security/SecurityFunctions/CheckCookies.cs:19](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Security/SecurityFunctions/CheckCookies.cs:19>), [Controllers/PageController/SearchPage.cs:47](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Controllers/PageController/SearchPage.cs:47>). В предикатах применяются перегрузки Equals/Contains со StringComparison и другие .NET-конструкции.

EF Core до версии 3 допускал клиентское вычисление в частях запроса; современный EF обычно отклоняет непереводимый предикат. Какие именно выражения переводит старый Pomelo, без запуска с MySQL здесь не установлено. [Документация EF](https://learn.microsoft.com/en-us/ef/core/querying/client-eval).

Переписать запросы с явной семантикой сравнения и collation; на новом провайдере проверить SQL и EXPLAIN. Не заменять всё механически на AsEnumerable: это может перенести фильтрацию целой таблицы в память.

### 23. P2 — глобальная уникальность URL не защищена одной операцией базы

**Подтверждено:** [PagesManagement/PagesManagementFunctions/SetUniqueAliasName.cs:23](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/PagesManagement/PagesManagementFunctions/SetUniqueAliasName.cs:23>), [Database/Context/CMSDataBase.cs:39](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Database/Context/CMSDataBase.cs:39>). Для проверки имени загружаются URL из трёх наборов. Между проверкой и сохранением другой запрос может выбрать тот же URL. Индексы заданы для отдельных сущностей; реальную физическую схему нужно сверить.

Создать единый реестр маршрутов с уникальным нормализованным путём и транзакционным резервированием. Сохранить детерминированную политику суффиксов и обработку конфликта. Хеш пути в текущем коде проверяется вместе с полной строкой, поэтому сам по себе его возможный конфликт не является подтверждённым обходом проверки.

### 24. P2 — синхронная БД, блокирующие ожидания и N+1-запросы

**Подтверждено:** [Database/DatabaseInteraction/Page/RefreshPageAndDependencies.cs:21](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Database/DatabaseInteraction/Page/RefreshPageAndDependencies.cs:21>), [TemplatesManagement/TemplatesManagementFunctions/TemplateRendering/GetChunks.cs:20](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/TemplatesManagement/TemplatesManagementFunctions/TemplateRendering/GetChunks.cs:20>), [Controllers/AdminPanelController/Users.cs:15](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Controllers/AdminPanelController/Users.cs:15>). Есть LoadAsync().Wait(), FirstOrDefaultAsync().Result и последовательная загрузка связанных сущностей. Рекурсивное обновление дерева умножает обращения к базе; списки админки часто выгружаются полностью.

Использовать последовательный async/await, проекции, пакетную загрузку связей и пагинацию. Передавать RequestAborted. Измерить SQL-запросы и время ответа на тестовом каталоге; обещать конкретное ускорение до измерений нельзя.

### 25. P2 — принудительный GC запускается на пути каждого запроса

**Подтверждено:** [Middlewares/ForcedGarbageCollectionMiddleware.cs:19](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Middlewares/ForcedGarbageCollectionMiddleware.cs:19>). Для каждого запроса создаётся не ожидаемая Task.Run; при пороге памяти вызывается GC.Collect. В предоставленной конфигурации порог установлен в 100 МБ. Сборка выдала CS4014.

Убрать middleware и сначала измерить память/аллокации. Частые полные сборки мусора и дополнительные задачи могут ухудшать задержки под нагрузкой; они не устраняют источник избыточных выделений памяти.

### 26. P2 — пагинация не всегда имеет устойчивую сортировку

**Подтверждено:** [Controllers/PageController/CategoryPage.cs:31](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Controllers/PageController/CategoryPage.cs:31>), [Controllers/PageController/SearchPage.cs:99](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Controllers/PageController/SearchPage.cs:99>). Сортировка по цене не имеет ThenBy(ID), поэтому товары с одинаковой ценой могут менять места. При одном поисковом варианте OrderBy вообще не добавляется перед Skip/Take.

Всегда задавать полный порядок. Для цены — Price и ID; для поиска — определённый рейтинг и ID либо просто ID. Отклонять некорректные значения пагинации и ограничивать размер страницы.

### 27. P2 — загрузки и генерация миниатюр недостаточно ограничены

**Подтверждено отсутствие прикладных лимитов:** [ImagesManagement/ImagesManagementFunctions/UploadProductImageToServer.cs:45](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/ImagesManagement/ImagesManagementFunctions/UploadProductImageToServer.cs:45>), [ImagesManagement/ImageHandler/ApplySettings.cs:29](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/ImagesManagement/ImageHandler/ApplySettings.cs:29>), [FileManagerManagement/FileManagerFunctions/UploadFileToServer.cs:42](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/FileManagerManagement/FileManagerFunctions/UploadFileToServer.cs:42>). Декодер загружает всё изображение без проверки пикселей и кадров; файловый менеджер доверяет расширению. Проверка существования имени не резервирует файл атомарно.

Стандартные лимиты сервера могут ограничивать байты запроса, но не гарантируют ограничение распакованной картинки. Установить лимиты байтов, размеров, кадров, выходных вариантов и числа параллельных декодирований. Проверять формат и сохранять через временный файл. Для одинаковой миниатюры нужна одна операция публикации.

JS/CSS в файловом менеджере — возможность управления исполняемым содержимым сайта. Это должно быть отдельное привилегированное право, а не автоматически следовать из права загрузить картинку.

### 28. P2 — обработчик конфликта создания пользователя стоит не вокруг SaveChanges

**Подтверждено:** [Database/DatabaseInteraction/User/AddUser.cs:36](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Database/DatabaseInteraction/User/AddUser.cs:36>). DbUpdateException ловится вокруг db.Users.Add, тогда как реальная запись и нарушение уникальности происходят в SaveChanges ниже.

Две одновременные операции могут обе пройти предварительную проверку, а вторая получить необработанную ошибку вместо 409. Ловить именно конфликт уникального индекса при сохранении. Прочие ошибки БД не скрывать под видом конфликта логина.

### 29. P2 — защита администратора привязана к ID=1; удаление типа может затронуть пользователей

**Подтверждено кодом, конкретный каскад зависит от фактической схемы:** [Database/DatabaseInteraction/User/DeleteUser.cs:14](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Database/DatabaseInteraction/User/DeleteUser.cs:14>), [Database/DatabaseInteraction/UserType/DeleteUserType.cs:14](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Database/DatabaseInteraction/UserType/DeleteUserType.cs:14>), [Database/Entities/User.cs:16](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Database/Entities/User.cs:16>).

Запрещены некоторые изменения только для записи с ID=1, хотя наличие максимального уровня определяется отдельно. Тип пользователя связан обязательной ссылкой, а явная политика DeleteBehavior не задана. Удаление типа с пользователями может каскадно удалить их или завершиться ошибкой внешнего ключа — проверить DDL.

Защищать системную роль и последнего администратора по смыслу, а не номеру. Запретить удаление занятой роли либо явно переназначать пользователей в транзакции. Связано с опасным bootstrap из пункта 03.

### 30. P2 — статистику посетителей можно подменять; запись посещений нагружает основной запрос

**Подтверждено:** [Controllers/PageController/SetVisitInfo.cs:23](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Controllers/PageController/SetVisitInfo.cs:23>). visitor_id принимается из cookie как обычный идентификатор записи, без подписи и проверки принадлежности. Можно записывать свои посещения на чужой ID. Уникальность по IP объединяет разных людей за NAT.

Каждый учитываемый просмотр пишет в MySQL; таблица VisitedPages может быстро расти. Определить цель статистики, использовать случайный идентификатор, ограничение частоты и очередь/агрегацию. Задать срок хранения и объём. Удаление Visitor очищает связанные события только при подходящей фактической политике FK.

### 31. P2 — sitemap формируется до учёта reverse proxy и без XML-кодирования

**Подтверждено:** [Startup.cs:85](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Startup.cs:85>), [Middlewares/SitemapMiddleware.cs:24](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Middlewares/SitemapMiddleware.cs:24>). UseSitemap расположен до UseForwardedHeaders. При TLS на Nginx sitemap может получить внутреннюю схему HTTP. Host берётся из запроса; корректность зависит также от настройки разрешённых Host и proxy. Content-Type XML не указан.

Перенести обработку доверенных forwarded headers в начало, использовать настроенный канонический адрес сайта и XML-сериализатор, указать Content-Type. При большом каталоге кешировать sitemap и делить его по допустимому размеру. Проверить KnownProxies/KnownNetworks и Host на сервере; без его настроек нельзя утверждать наличие обхода.

### 32. P2 — шаблонизатор неустойчив к корректному многострочному тексту и циклам

**Воспроизведено:** [TemplatesManagement/TemplatesManagementFunctions/TemplateRendering/ReplaceConditionalExpressionValue.cs:112](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/TemplatesManagement/TemplatesManagementFunctions/TemplateRendering/ReplaceConditionalExpressionValue.cs:112>). Вставляемый текст экранирует обратные слеши и кавычки, но не переводы строк. Многострочный текст внутри условного блока превращается в обычную строку C# с буквальным переносом и может сорвать Razor-компиляцию.

Дополнительно: [TemplatesManagement/TemplatesManagementFunctions/TemplateRendering/CreateInsertionReplacementsCollection.cs:61](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/TemplatesManagement/TemplatesManagementFunctions/TemplateRendering/CreateInsertionReplacementsCollection.cs:61>) в отрицательной ветке Product:CurrentPrice обращается к Model as ProductPage без гарантии типа. [TemplatesManagement/TemplatesManagementFunctions/TemplateRendering/GetChunks.cs:18](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/TemplatesManagement/TemplatesManagementFunctions/TemplateRendering/GetChunks.cs:18>) запрещает лишь прямую ссылку чанка на себя, но не цикл A → B → A.

Нужны ограниченная грамматика/AST, проверка типов, граф зависимостей без циклов и ошибки до публикации. Простое экранирование @ в SourceToCSHTML снижает риск прямого ввода Razor, но не является доказанным полноценным sandbox. Удалённое выполнение произвольного C# через шаблон в этом аудите не подтверждено.

## Архитектура и качество кода

### 33. P3 — сложность сосредоточена в partial-классах и неявном состоянии

**Подтверждено:** [Controllers/AdminPanelController/AdminPanel(POST).cs:38](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Controllers/AdminPanelController/AdminPanel(POST).cs:38>), [PagesManagement/PagesManagementFunctions/PageModelToPage.cs:17](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/PagesManagement/PagesManagementFunctions/PageModelToPage.cs:17>), [SettingsManagement/ConfigurationHandler/CTOR.cs:15](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/SettingsManagement/ConfigurationHandler/CTOR.cs:15>).

Контроллеры распределены по множеству partial-файлов, но остаются большим диспетчером по PageId. Статические функции смешивают бизнес-правила, EF, HTTP, файловую систему и локализованный текст журналов. HttpContext.Items служит неявной связью между логикой и Razor, а конфигурация перечитывается для новых объектов. Пустые catch скрывают дефекты.

Предлагаемое разделение внутри одного приложения:

- Контроллеры: HTTP, проверка ввода, права и ответы.
- Сервисы: страницы/каталог, пользователи, публикация шаблонов, изображения, статистика.
- Инфраструктура: EF и MySQL, файловое хранилище, конфигурация, фоновые задачи.
- Типизированные модели представлений вместо соглашений о строковых ключах HttpContext.Items.

Для этого проекта достаточно модульного монолита. Микросервисы и собственная универсальная обёртка над каждым DbSet сейчас добавят сложность без доказанной пользы.

Дополнительные замечания:

- Пути собираются строками и обрезаются по длине префикса; Directory.GetFiles на Windows может вернуть обратные слеши, а код заменяет только /. Нужны Path.Combine/GetRelativePath и отдельное построение URL: [FileManagerManagement/FileManagerFunctions/GetFolders.cs:24](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/FileManagerManagement/FileManagerFunctions/GetFolders.cs:24>).
- HasAccessToFolder использует Contains, а не нормализованную проверку принадлежности каталогу: [FileManagerManagement/FileManagerFunctions/HasAccessToFolder.cs:14](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/FileManagerManagement/FileManagerFunctions/HasAccessToFolder.cs:14>). Regex на внешних путях ограничивает обычный ../, поэтому подтверждённый универсальный path traversal здесь не заявляется. Проверить также symlink/junction и FileManager/image FullPath.
- Цены хранятся как uint: [Database/Entities/Pages/ProductPage.cs:10](<C:/Users/user/Documents/Codex/StoreCMS/StoreCMS/Database/Entities/Pages/ProductPage.cs:10>). Если это целые рубли, это допустимое существующее правило; если нужны копейки, явно перейти к decimal и валюте, согласовав масштаб данных.
- DateTime.Now для сессий и журналов зависит от часов и зоны сервера. Использовать UTC/DateTimeOffset и заданную зону отображения, отдельно определить границу дня статистики.
- Версия записи/concurrency token не задана: два редактора могут перезаписать изменения друг друга. На значимых формах добавить оптимистическую проверку версии.
- В репозитории отслеживаются сборочные артефакты и настройки IDE. Убрать их из дальнейшего версионирования, настроить исключения; не удалять пользовательские файлы во время аудита.
- Добавить понятную обработку ошибок production, корреляцию событий, журнал предупреждений и мониторинг. Отсутствие UseHsts/UseHttpsRedirection в приложении не доказывает отсутствие HTTPS на Nginx.
- Автоматические тесты и CI не обнаружены. Их нужно добавить для перечисленных регрессий и основных пользовательских сценариев.

## Что уже сделано разумно

Работа с данными преимущественно идёт через LINQ/EF; конкатенация пользовательского текста в raw SQL в проверенных исходниках не найдена. Это снижает риск классической SQL-инъекции, но не заменяет проверку переводимого SQL.

Действия админки централизованно проверяют пользователя и уровень доступа. Неизвестные действия и отсутствующие уровни доступа закрываются максимальным уровнем. В текущем config все действия выставлены на уровень 5. Вспомогательные методы контроллеров в основном помечены NonAction.

Есть уникальные индексы, AsNoTracking для чтения, освобождение изображений и потоков через using, нормализация имён и ограничения расширений. Используется собственная стабильная функция хеширования строк, а не непостоянный между процессами string.GetHashCode. Эти решения стоит сохранить или заменить осмысленно, а не выкидывать вместе со всем проектом.

## План модернизации

### Этап 1 — исправить опасные дефекты и подготовить восстановление

Сначала устранить пункты 03–08, 13–18 и уязвимые зависимости. Удаление изображений и гонки шаблонов требуют небольших локальных исправлений; аутентификация потребует отдельной совместимой стратегии переноса.

Для действующего сайта подготовить резервную копию БД и Storage/шаблонов с проверенным восстановлением. Нужны оба вида данных: одна SQL-копия не восстановит файловые изображения и сгенерированные представления. Не запускать аудитируемую программу на рабочей базе: старт имеет побочные эффекты.

Критерий завершения: нет фиксированного bootstrap-пароля, изменяющие запросы защищены, а чужое изображение не затрагивается при операции над своим; параллельное сохранение не использует общий контекст/список.

### Этап 2 — зафиксировать текущее поведение на тестовой копии

Собрать набор сценариев: вход/выход, смена пароля, роли, страницы, категории, товары, поиск, изображения, шаблоны, редиректы, 404, статистика и архивирование.

Получить схему без данных и секретов: SHOW CREATE TABLE/индексы, версию MySQL, charset/collation, sql_mode и правила FK. Проверить размеры таблиц и несколько EXPLAIN на обезличенной копии. Это потребуется для завершения проверки MySQL; пароль действующего сервера не нужен.

Зафиксировать URL, опубликованность, цены, типы страниц, идентификаторы и внешний вид ключевых страниц. Измерить число SQL-запросов, p95 времени ответа, CPU и память. Установить допустимый регресс относительно измеренного baseline.

### Этап 3 — выбрать совместимый стек .NET и MySQL

**Конечная цель — .NET 10 / ASP.NET Core 10.** Совместимость ORM нельзя вывести только из возможности запуска приложения на этом runtime.

В опубликованной таблице Pomelo проверен релиз 9.0.0 для EF Core 9.0.x; релиз для EF Core 10 в этой таблице не указан. Не планировать Pomelo 9 + EF Core 10 как совместимую пару. [Таблица Pomelo](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql#compatibility).

Oracle объявляет поддержку .NET 10 и EF Core 10 начиная с Connector/NET 9.6. Кандидат для проверки — ASP.NET Core 10 + EF Core 10 + совместимый стабильный MySql.EntityFrameworkCore. Это смена провайдера, поэтому её нужно подтвердить на реальной схеме, LINQ-запросах, миграциях и транзакциях; до этих проверок выбор предварительный. [Официальный репозиторий MySQL](https://github.com/mysql/mysql-connector-net#mysql-connectornet).

Вариант сохранения Pomelo с EF Core 9 можно использовать как краткий внутренний промежуточный этап, но не как долгосрочный поддерживаемый результат. Версии runtime, ORM, провайдера и сервера нужно фиксировать совместно. Промежуточные старые .NET не нужно последовательно развёртывать на production.

### Этап 4 — перенести hosting, маршруты и инфраструктуру

В отдельной ветке/копии перенести запуск на современный host, регистрацию служб и endpoint routing. Сохранить catch-all поведения публичных страниц и маршрут /admin; бизнес-поиск страницы убрать из IRouteConstraint в отдельный сервис.

Правильно упорядочить proxy headers, обработку ошибок, статику, аутентификацию и авторизацию. Вынести настройки в IOptions с проверкой на запуске; удалить BuildServiceProvider и ForcedGC. Переписать фоновые задачи с отменой и наблюдаемыми ошибками.

Не объединять этот перенос с массовым изменением дизайна сайта: сначала добиться функционального совпадения и тестов.

### Этап 5 — перенести ORM и схему без потери данных

Сначала сверить модель EF со старой физической схемой. Особое внимание — наследованию Page и отдельным таблицам UsualPage/CategoryPage/ProductPage: современная автоматическая стратегия отображения не должна самопроизвольно менять таблицы и смысл ID.

Явно задать таблицы, ключи, длины строк, charset/collation, FK, точность денег и политику удаления. При необходимости разделить наследование C# для представлений и отдельные EF-сущности, сохранив существующие ключи и PageType.

Создать baseline миграций на копии, затем просмотренный SQL изменения схемы. Проверить дубли URL/логинов, максимальные длины индексируемых строк, расхождения регистров и связанные изображения. Runtime-аккаунту БД дать только необходимые права работы с данными; DDL выполнять отдельной процедурой.

Не удалять и не пересоздавать production-базу для перехода с EnsureCreated на миграции.

### Этап 6 — обновить шаблоны, изображения и аутентификацию

Для шаблонов предпочтителен ограниченный интерпретатор собственных маркеров с типизированной моделью и проверяемым графом чанков. Если временно сохранить генерацию .cshtml, нужна отдельная проверка современного механизма компиляции, синтаксиса и атомарной публикации.

Шаблон и обычный текст должны иметь разные правила доверия. Администратор, публикующий HTML/JS, получает практически право изменять поведение сайта; роль контент-редактора не должна автоматически получать это право.

Обновить ImageSharp, обеспечить лимиты и безопасное именование производных файлов. Перенести вход с поэтапным обновлением хешей, отзывом старых сессий и надёжным хранением Data Protection keys, если выбран штатный механизм cookies.

### Этап 7 — проверить развёртывание и откат

На копии выполнить миграцию, сравнить количество и ключи записей, маршруты, поиск, страницы и изображения. Проверить одновременные сохранения, отсутствие БД, недоступный диск, неверный JSON и аварийный перезапуск фоновой задачи.

На окружении с Nginx проверить HTTPS, канонический host, sitemap, адрес клиента, лимиты загрузок, cookies и секреты. Следить за 500-ошибками, памятью, latency и отсутствующими файлами.

Для первого переключения проще предусмотреть короткое окно остановки записи и согласованный снимок БД+файлов. Откат должен включать совместимость схемы, а не только старый бинарный файл. Возвращение на старое приложение после несовместимой миграции без восстановления базы может повредить данные.

## Минимальный набор регрессионных проверок

| Проверка | Ожидаемое поведение после исправления |
|---|---|
| Удаление производных a.jpg при наличии ba.jpg | ba.jpg и его производные остаются |
| Одновременная генерация двух шаблонов | Замены не смешиваются, DbContext не используется конкурентно |
| Название страницы содержит кавычки/HTML | Обычный текст кодируется; HTML разрешён только по политике |
| Изменяющий запрос без CSRF-токена | Отклонён |
| Смена пароля | Старые сессии отозваны |
| Два добавления одинакового логина | Один успех, один 409, без необработанного исключения |
| Сохранение настройки с кавычкой/переносом | Валидный JSON либо понятный отказ валидации |
| Событие журнала A & B | Валидный структурированный документ |
| Повторное архивирование/перезапуск | Нет потери или повторного удаления событий |
| Сбой диска после изменения БД | Операция восстанавливается или явно компенсируется |
| Одно поисковое выражение, несколько страниц | Полный устойчивый порядок результатов |
| Переименование родительской страницы | URL/хлебные крошки потомков обновлены согласованно |
| Цикл чанков и многострочный условный блок | Ошибка до публикации либо корректный результат |
| Большое/многокадровое изображение | Ограниченный расход ресурсов, понятный отказ |
| Удаление занятой роли/последнего администратора | Запрещено либо выполняется явное безопасное переназначение |

## Что остаётся проверить в окружении

Версию и DDL MySQL, реальные планы запросов, политики FK/collation, HTTPS и доверенные proxy, права файловой системы, публикацию и runtime-компиляцию шаблонов, фактическую доступность /admin извне, нагрузку и механизм резервного копирования. Эти пункты не объявлены проверенными по исходникам.

Отчёт не требует секретов. Следующий практический шаг — исправления P1 в отдельной копии с тестовой базой, затем перенос на выбранный совместимый стек.
