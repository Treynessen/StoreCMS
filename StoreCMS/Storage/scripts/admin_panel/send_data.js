function createSendDataEventHandler(requestMethod, formId, responseHandler, validationErrorHandler) {
    return function (e) {
        let formElement = document.getElementById(formId);
        if (formElement.checkValidity()) {
            let formData = new FormData();
            for (let element of formElement.elements) {
                formData.append(element.name, element.value);
            }
            let request = new XMLHttpRequest();
            request.open(requestMethod, location.origin + location.pathname, false);
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