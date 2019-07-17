function createSendDataEventHandler(requestMethod, requestSearchString, formId, successfulRequestHandler, errorHandler) {
    let errorProcessed = false;
    let requestString = location.origin + location.pathname + requestSearchString;
    return function (e) {
        let success = false;
        let formElement = document.getElementById(formId);
        if (formElement.checkValidity()) {
            let formData = new FormData();
            for (let element of formElement.elements) {
                formData.append(element.name, element.value);
            }
            let request = new XMLHttpRequest();
            request.open(requestMethod, requestString, false);
            request.send(formData);
            if (request.status == 200 || request.status == 201) {
                success = true;
                if (typeof successfulRequestHandler === 'function')
                    successfulRequestHandler(request);
            }
        }
        if (!success && !errorProcessed) {
            if (typeof errorHandler === 'function')
                errorHandler(formElement);
            errorProcessed = true;
        }
        if (e !== 'undefined') {
            e.stopImmediatePropagation();
            e.preventDefault();
        }
    }
}