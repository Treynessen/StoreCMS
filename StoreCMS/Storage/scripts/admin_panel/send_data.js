function getAdminUserValidationMessage(request) {
    if (!request || (request.status !== 422 && request.status !== 409)) return null;
    let messages = request.status === 409 ? {
        user_login_taken: 'Пользователь с таким логином уже существует. Выберите другой логин.'
    } : {
        user_data_required: 'Заполните обязательные данные пользователя.',
        user_login_required: 'Введите логин.',
        user_login_invalid: 'Логин может содержать только латинские буквы, цифры, дефис и знак подчёркивания.',
        user_password_required: 'Введите пароль.',
        user_password_invalid: 'Пароль должен содержать не менее 5 символов: латинские буквы, цифры или -_!@#$%&?*№~=. Пароль admin запрещён без учёта регистра. Пробелы и другие символы недопустимы.',
        user_role_required: 'Выберите тип пользователя.',
        user_role_invalid: 'Выбранный тип пользователя не существует. Обновите список типов и выберите существующий.',
        user_id_required: 'Не указан корректный идентификатор пользователя. Откройте форму пользователя заново.',
        user_idle_time_invalid: 'Время бездействия должно быть от 10 до 10080 минут.'
    };
    try {
        if (!/application\/json/i.test(request.getResponseHeader('Content-Type') || '')) return null;
        let data = JSON.parse(request.responseText);
        return data && typeof data.error === 'string' && Object.prototype.hasOwnProperty.call(messages, data.error)
            ? messages[data.error] : null;
    }
    catch (error) { return null; }
}

function setAdminPanelCsrfHeader(request, method) {
    if (/^(GET|HEAD|OPTIONS)$/i.test(method)) return true;
    let meta = document.querySelector('meta[name="csrf-token"]');
    if (!meta || !meta.content) {
        alert('Форма устарела или отсутствует защитный токен. Сохраните введённые данные и обновите страницу.');
        return false;
    }
    request.setRequestHeader('X-CSRF-TOKEN', meta.content);
    return true;
}

// [Комментарий ИИ-агента]: Enter и requestSubmit проходят прежнюю проверку кнопки; нативный GET с токеном исключён также явным method=post в Razor.
document.addEventListener('submit', function (event) {
    if (event.defaultPrevented) return;
    let form = event.target;
    if (!form.hasAttribute('data-admin-ajax')) return;
    event.preventDefault();
    let button = event.submitter || form.querySelector('input[type="submit"]:not(:disabled), button[type="submit"]:not(:disabled)');
    if (button) button.click();
});

function showAdminPanelHttpError(request, formElement) {
    let status = request ? request.status : 0;
    let message;
    switch (status) {
        case 0:
            message = 'Не удалось получить ответ сервера. Результат операции неизвестен. Проверьте состояние перед повторной отправкой.';
            break;
        case 400:
            message = 'Запрос отклонён (HTTP 400). Проверьте данные: форма могла устареть. Сохраните введённые данные перед обновлением страницы.';
            break;
        case 401:
            message = formElement && formElement.id === 'login-form'
                ? 'Не удалось войти. Проверьте логин и пароль.'
                : 'Сессия отсутствует или истекла. Сохраните введённые данные и войдите снова.';
            break;
        case 403:
            message = 'Операция запрещена. Проверьте права доступа и, если требуется, текущий пароль.';
            break;
        case 404:
            message = 'Запрошенный объект не найден (HTTP 404).';
            break;
        case 409:
            message = getAdminUserValidationMessage(request) || 'Конфликт данных (HTTP 409). Проверьте, не существует ли уже такой логин и не изменены ли данные другим запросом.';
            break;
        case 415:
            message = 'Сервер отклонил формат загружаемого файла (HTTP 415).';
            break;
        case 422:
            message = getAdminUserValidationMessage(request) || 'Данные не приняты (HTTP 422). Проверьте заполненные поля.';
            break;
        default:
            message = status >= 500
                ? 'Ошибка сервера (HTTP ' + status + '). Результат операции не подтверждён. Проверьте состояние перед повторной отправкой.'
                : 'Получен неожиданный ответ сервера (HTTP ' + status + '). Успех операции не подтверждён.';
    }
    alert(message);
}

function handleAdminPanelHttpResponse(request, formElement, responseHandler, successStatus) {
    let contentType = request.getResponseHeader('Content-Type') || '';
    let unexpectedHtml = request.method !== 'get' && /text\/html|application\/xhtml\+xml/i.test(contentType);
    if (request.status !== successStatus || unexpectedHtml) {
        showAdminPanelHttpError(request, formElement);
        return;
    }
    if (typeof responseHandler === 'function') {
        responseHandler(request, formElement);
    }
}

function createSendDataEventHandler(requestMethod, formId, responseHandler, validationErrorHandler, successStatus = 200) {
    return function (e) {
        e.preventDefault();
        let formElement = document.getElementById(formId);
        if (formElement.checkValidity()) {
            let formData = new FormData();
            let searchString = ''; // Для GET-методов
            let isGetMethod = requestMethod.toLowerCase() == 'get';
            for (let element of formElement.elements) {
                if (element.name === '__RequestVerificationToken' && isGetMethod) continue;
                if (isGetMethod) {
                    if (searchString == '')
                        searchString = '?' + element.name + '=' + element.value;
                    else searchString += '&' + element.name + '=' + element.value;
                }
                else formData.append(element.name, element.value);
            }
            let request = new XMLHttpRequest();
            request.method = requestMethod.toLowerCase();
            try {
                request.open(requestMethod, location.origin + location.pathname + searchString, false);
                if (request.method === 'post') request.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
                if (!setAdminPanelCsrfHeader(request, requestMethod)) return;
                request.send(formData);
            }
            catch (error) {
                showAdminPanelHttpError(null, formElement);
                return;
            }
            handleAdminPanelHttpResponse(request, formElement, responseHandler, successStatus);
        }
        else if (typeof validationErrorHandler === 'function') {
            validationErrorHandler(formElement);
        }
    }
}
