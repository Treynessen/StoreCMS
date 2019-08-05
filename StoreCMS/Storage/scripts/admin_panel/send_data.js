function createSendDataEventHandler(requestMethod, formId, responseHandler, validationErrorHandler) {
    return function (e) {
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
            request.open(requestMethod, location.origin + location.pathname + searchString, false);
            request.send(formData);
            if (typeof responseHandler === 'function') {
                responseHandler(request, formElement);
            }
        }
        else if (typeof validationErrorHandler === 'function') {
            validationErrorHandler(formElement);
        }
        e.preventDefault();
    }
}