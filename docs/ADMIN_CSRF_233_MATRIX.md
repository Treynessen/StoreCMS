# Матрица CSRF 2.3.3

Дата анализа до реализации: 18.10.2026. HEAD: 3708f65c9b43918571789935746ea14f17e741a5, master.

Все маршруты: `/admin` (также текущий путь панели при альтернативном mapping/PathBase); селектор `pageID` передаётся в форме. 27 POST + 2 PUT + 12 DELETE = 41. Перечень сверён с SupportsAdminPanelMutation, диспетчерами и клиентской матрицей 2.3.2. Обычная смена пароля является ветвью EditUserData, отдельного маршрута нет.

На входе 2.3.2 все пары уже передают X-CSRF-TOKEN; HTML-форма ChangeInitialPassword передаёт hidden __RequestVerificationToken. В 38 объявлениях изменяющих форм есть hidden, три оболочки выдают meta, inline logout передаёт header. PUT передаёт multipart с файлом. План: штатный ValidateRequestAsync централизованно на контроллере после прежних guard, до action; без токена/с недействительным токеном 400, если запрос дошёл до CSRF.

| Маршрут | Метод | Операция | Авторизация | Токен на входе | План проверки |
|---|---|---|---|---|---|
| /admin; pageID=54 | POST | LoginForm | без сессии; с действующей сессией сохраняется bootstrap-запрет | header | обязательная IAntiforgery до action |
| /admin; pageID=59 | POST | ChangeInitialPassword | действующая сессия; bootstrap разрешён | header + native hidden | обязательная IAntiforgery до action |
| /admin; pageID=13 | POST | AddPage | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=14 | POST | EditPage | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=16 | POST | AddCategory | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=18 | POST | EditCategory | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=20 | POST | AddProduct | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=21 | POST | EditProduct | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=26 | POST | AddRedirection | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=27 | POST | EditRedirection | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=29 | POST | AddTemplate | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=30 | POST | EditTemplate | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=32 | POST | AddChunk | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=33 | POST | EditChunk | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=36 | POST | CreateFolder | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=37 | POST | CreateStyle | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=38 | POST | CreateScript | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=39 | POST | EditStyle | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=40 | POST | EditScript | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=42 | POST | AddUser | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=43 | POST | EditUser | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=46 | POST | AddUserType | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=47 | POST | EditUserType | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=49 | POST | AddSynonymForString | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=50 | POST | EditSynonymForString | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=52 | POST | EditUserData | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=53 | POST | EditSettings | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=24 | PUT | AddProductImage | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=35 | PUT | UploadFile | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=58 | DELETE | Exit | идемпотентно, в том числе без сессии | header | обязательная IAntiforgery до action |
| /admin; pageID=15 | DELETE | DeletePage | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=17 | DELETE | DeleteCategory | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=22 | DELETE | DeleteProduct | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=25 | DELETE | DeleteProductImage | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=28 | DELETE | DeleteRedirection | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=31 | DELETE | DeleteTemplate | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=34 | DELETE | DeleteChunk | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=41 | DELETE | DeleteFileOrFolder | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=44 | DELETE | DeleteUser | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=48 | DELETE | DeleteUserType | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |
| /admin; pageID=51 | DELETE | DeleteSynonymForString | действующая сессия + HasAccessTo | header | обязательная IAntiforgery до action |

## Порядок и границы

HTTPS 403 → CheckCookies (401 для защищённых операций) → selector/method 400 при действующей сессии → bootstrap/HasAccessTo 403 → ValidateRequestAsync 400 → action. Login и идемпотентный Exit также требуют токен; отсутствие сессии не является исключением для них. GET/HEAD/OPTIONS не валидируются этим guard. При отзыве/истечении сессии защищённый запрос даёт 401 даже при плохом CSRF; logout с валидной остаточной парой остаётся 200. После нового входа старый токен вкладки даёт 400, нужен полный GET без автоматического повтора.

Боковые эффекты GET для 2.3.5: CheckCookies/SessionStore продлевает LastActionUtc и может отзывать истёкшую сессию; ClearLegacy пишет cookies. ImageTagHelper → ImageHandler.ApplySettings создаёт производные файлы и метаданные (включая административные списки изображений). Публичные PageController/статистика записывают посещения. Выдача antiforgery-cookie при GET необходима протоколу. Добавление проверки POST/PUT/DELETE эти эффекты GET не защищает.

## Результат выполнения плана, 18.10.2026

Проверка централизованно включена в AdminAntiforgeryValidation.cs. Все 41 строки прошли HTTPS-проверку guard на двух маршрутах с корректным header/hidden multipart и без/с пустым/повреждённым/подменённым токеном; отдельно для всех 41 проверены отсутствующая и другая antiforgery-cookie. Корректная пара достигала тестовой границы action (204); это не утверждение об успешном CRUD каждой сущности. В отсутствие токена на обеих маршрутах также проверен отказ перед настоящим action без перехватчика. Бизнес-снимки при отказе неизменны, служебные сессионные эффекты сохраняются. Реально выполненные PNG/файловые действия и auth/password-потоки перечислены в PROGRESS.md и README набора. Независимое ревью и выборочная ручная приёмка владельца в Opera завершены 18.10.2026; новых подтверждённых P0–P3 нет. Ручная проверка всех 41 строк не заявляется. Разбор POST pageID=1 → 401 подтвердил прежний контракт: вход — pageID=54. Объём ручной приёмки — PROGRESS.md. Владелец разрешил финальные проверки, один коммит 2.3.3 и push.
