function checkboxEventHandler(e) {
    let currentTarget = e.currentTarget;
    if (currentTarget.checked)
        currentTarget.setAttribute('value', 'true');
    else currentTarget.setAttribute('value', 'false');
}