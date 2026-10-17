# Архитектура StoreCMS

Снимок: 2026-10-14. Ниже — факты из текущего кода. Пути относительно корня репозитория; наличие пути в коде не означает наличие файла в рабочей папке.

## Платформа и зависимости

- Решение: `StoreCMS.sln`; единственный проект приложения: `StoreCMS/StoreCMS.csproj`; namespace — `Treynessen`.
- ASP.NET Core MVC/Razor, целевая платформа `netcoreapp2.2`. Точная версия runtime действующего сервера неизвестна.
- Прямые пакеты: `Microsoft.AspNetCore.App` (без явной версии), `Microsoft.AspNetCore.Razor.Design` 2.2.0, `Pomelo.EntityFrameworkCore.MySql` 2.2.0, `SixLabors.ImageSharp` 1.0.0-beta0006.
- Конфигурация выбирает MySQL; `Startup.cs` содержит также ветку SQL Server. Работа SQL Server не проверена.

## Карта компонентов

Все пути таблицы начинаются с `StoreCMS/`.

| Компонент | Назначение и точки входа |
|---|---|
| Запуск/DI | `Program.cs`: host, web-root Storage, первичная настройка. `Startup.cs`: зависимости, middleware, MVC-маршруты |
| Публичные страницы | `RequestManagement/RequestConstraint.cs`, `RequestManagement/RequestHandler/FindPage.cs`, `RequestManagement/RequestHandler/GetRedirection.cs`; `Controllers/PageController/`: обычные страницы, категории, товары, поиск, 404, посещения |
| Админка | `Controllers/AdminPanelController/`: один partial-контроллер, диспетчеры `AdminPanel(GET/POST/PUT/DELETE).cs`; модели в `AdminPanelTypes/`, представления в `Views/AdminPanel/` |
| Данные | `Database/Context/CMSDataBase.cs`: класс CMSDatabase. `Database/Entities/`: страницы, товары, шаблоны/чанки, пользователи/роли/сессии, изображения, редиректы, синонимы, посещения и журналы |
| Операции БД | `Database/DatabaseInteraction/`: статические partial-функции CRUD, также выполняющие операции с файлами и журналами |
| Страницы | `PagesManagement/PagesManagementFunctions/`: преобразование моделей, URL/псевдонимы, хлебные крошки и дерево зависимостей |
| Доступ | `Security/SecurityFunctions/`: вход, пароль, сессии/cookies, права. `Security/AccessLevelConfiguration.cs`: числовые уровни действий |
| Шаблоны | `TemplatesManagement/TemplatesManagementFunctions/TemplateRendering/SourceToCSHTML.cs`: маркеры → Razor; `GetChunks.cs`: ссылки на чанки; `WriteCshtmlContentToFile.cs`: запись представлений |
| Файлы/изображения | `FileManagerManagement/`: каталоги, загрузки, CSS/JS. `ImagesManagement/`: загрузка, размеры/сжатие, производные изображения. `TagHelpers/`: вывод изображений и пагинации |
| Настройки | `SettingsManagement/ConfigurationHandler/`: чтение и перезапись JSON; `Extensions/HostingEnvironmentExtensions/`: вычисление путей |
| Журналы/фон | `LogManagement/`: файловые ошибки и события админки. `TimedHostedService/`: ежедневный перенос событий БД в файлы и очистка посетителей |
| Вспомогательные | `Localization/`: интерфейсы и русские реализации; `Translit/`: транслитерация; `Functions/`: общие функции |

## Основные взаимодействия

1. **Запуск:** Program → Startup/DI → CMSDatabase. Конструктор контекста вызывает EnsureCreated; Program создаёт роль максимального уровня и пользователя admin/admin, если нет пользователя такого уровня. Это возможно и при последующих запусках. Обязательная смена пароля кодом не обеспечена.
2. **HTTP:** sitemap → forwarded headers → принудительный GC → статика → MVC (`Startup.cs`). `/search` идёт в SearchPage; catch-all с RequestConstraint ищет опубликованную страницу/редирект; далее `/admin`; последняя catch-all ветка — 404.
3. **Публичная страница:** RequestConstraint → MySQL через EF → RequestedPage/Redirection в HttpContext.Items → PageController → шаблон Razor или содержимое. Категория/поиск отдельно получают товары; посещения записываются в БД.
4. **Админка:** диспетчер по HTTP-методу и PageId → проверка сессии/уровня (вход — отдельная ветка) → функции управления → EF/файлы/журнал → HTTP-ответ. Данные для представлений часто передаются через HttpContext.Items.
5. **Шаблон:** исходный текст в сущности Template/Chunk → замена маркеров/условий/ссылок → файл .cshtml → Razor. Генерация использует общий static-список; некоторые операции чанков запускают параллельные ветки с одним DbContext.
6. **Фоновая задача:** Timer → новый DI scope → архив событий пользователей в XML → удаление перенесённых записей → очистка посетителей.

## Хранилища и границы

- `StoreCMS/Settings/config.json`: БД, уровни доступа и настройки поведения; содержит секрет подключения — значения не документировать.
- `StoreCMS/Storage/`: фактический web-root, публичные файлы. `wwwroot` указан в csproj, но Program выбирает Storage.
- `StoreCMS/Views/Templates/`, `Views/Chunks/`: пути генерируемых представлений; `Settings/product_block.cshtml`: вычисляемый путь блока товара. Его наличие не подтверждено текущим деревом.
- `StoreCMS/Logs/`: ошибки и архивы админских событий, путь вычисляется расширениями.
- БД и файловая система изменяются раздельно; единой атомарной фиксации операций нет. DbContext зарегистрирован scoped; конфигурация scoped; фон создаёт свой scope.

## Не установлено

Версия/DDL/collation/FK MySQL, production runtime, Nginx/HTTPS, права файлов, процесс публикации и runtime-компиляция Razor. Миграции, SQL-схема, автоматические тесты и CI-файлы при аудите не найдены. Эти сведения требуют отдельной проверки; устройство кода их не доказывает.

## Дополнение 2.3.2 - выдача antiforgery, 2026-10-17

Базовый снимок выше исторический. В текущем фрагменте Startup регистрирует штатный IAntiforgery ASP.NET Core 2.2 через Security/AdminAntiforgery.cs. Отдельная cookie StoreCMS.Antiforgery.v1: Secure Always, HttpOnly, SameSite=Lax, Path=/, Domain отсутствует, сессионный срок. Имена session/antiforgery раздельны. HTTP-guard и no-store контроллера действуют до Razor.

IAntiforgeryAdditionalDataProvider возвращает версию и SHA-256 предъявленной канонической StoreCMS.Session.v2 либо anonymous-маркер. Значение помещается внутрь защищённого штатного request-token. Provider не аутентифицирует, не обращается к БД, не меняет скользящий срок и не вводит абсолютный срок. Сохраняются D08/D09; отзыв в БД закрывает доступ даже при криптографически валидной остаточной паре.

Views/AdminPanel/_Antiforgery.cshtml выдаёт meta request-token трём оболочкам: LoginForm, CommonPage, ChangeInitialPassword. В изменяющих формах явный Html.AntiForgeryToken и method=post, в GET-формах токена нет. Общие send_data/send_file и inline logout передают X-CSRF-TOKEN; FormData и бизнес-параметры сохранены. После изменения авторизации токен получается полным GET. Старая вкладка после нового входа требует ручного обновления; автоматического повтора мутаций нет.

Обязательная проверка CSRF не включена: этот фрагмент готовит выдачу и передачу для независимого ревью 2.3.2. Тестовая проверка пары существует только в отдельном host tests/AdminAntiforgeryRegression, не в action приложения. Изображения через GET, публичная статистика, платформа, SQL и хранение сессий не изменены.
