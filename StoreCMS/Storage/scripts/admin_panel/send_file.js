function createSendFileEventHandler(formId, responseHandler, errorHandler, ...availableExtensions) {
    for (let i in availableExtensions)
        availableExtensions[i] = availableExtensions[i].toLowerCase();
    return function (e) {
        let formElement = document.getElementById(formId);
        let inpitFileButton = e.currentTarget;
        if (inpitFileButton.files.length > 0) {
            let fileName = inpitFileButton.files[0].name.toLowerCase();
            let correctExtension = false;
            for (let extension of availableExtensions) {
                if (fileName.endsWith(extension)) {
                    correctExtension = true;
                    break;
                }
            }
            if (correctExtension) {
                let request = new XMLHttpRequest();
                request.open('PUT', location.origin + location.pathname, false);
                let data = new FormData();
                data.append(inpitFileButton.name, inpitFileButton.files[0], fileName);
                for (let element of formElement.elements) {
                    data.append(element.name, element.value);
                }
                request.send(data);
                if (typeof responseHandler === 'function') {
                    responseHandler(request);
                }
            }
            else if (typeof errorHandler === 'function') {
                errorHandler();
            }
        }
        e.preventDefault();
    }
}