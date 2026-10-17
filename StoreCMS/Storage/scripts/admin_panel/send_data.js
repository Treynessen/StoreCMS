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
            message = 'Конфликт данных (HTTP 409). Проверьте, не существует ли уже такой логин и не изменены ли данные другим запросом.';
            break;
        case 415:
            message = 'Сервер отклонил формат загружаемого файла (HTTP 415).';
            break;
        case 422:
            message = 'Данные не приняты (HTTP 422). Проверьте заполненные поля.';
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