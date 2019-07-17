function createSendFileEventHandler(requestSearchString, successfulRequestHandler, errorHandler, ...availableExtensions) {
    for (let i in availableExtensions)
        availableExtensions[i] = availableExtensions[i].toLowerCase();
    return function (e) {
        let successfulRequest = false;
        let inpitFileButton = e.currentTarget;
        if (inpitFileButton.files.length > 0) {
            let fileName = inpitFileButton.files[0].name.toLowerCase();
            let incorrectExtension = true;
            for (let extension of availableExtensions) {
                if (fileName.endsWith(extension)) {
                    incorrectExtension = false;
                    break;
                }
            }
            if (!incorrectExtension) {
                let request = new XMLHttpRequest();
                request.open('PUT', location.origin + location.pathname + requestSearchString, false);
                let data = new FormData();
                data.append(inpitFileButton.name, inpitFileButton.files[0], fileName);
                request.send(data);
                if (request.status == 200 || request.status == 201) {
                    if (typeof successfulRequestHandler === 'function')
                        successfulRequestHandler(request);
                    successfulRequest = true;
                }
            }
        }
        if (!successfulRequest && typeof errorHandler === 'function')
            errorHandler();
        if (e !== 'undefined') {
            e.preventDefault();
            e.stopImmediatePropagation();
        }
    }
}