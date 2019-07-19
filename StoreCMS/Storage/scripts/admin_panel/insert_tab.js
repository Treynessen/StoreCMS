function insertTabEventHandler(e) {
    if (e.keyCode == 9 && !e.shiftKey && !e.ctrlKey && !e.altKey) {
        let textarea = e.currentTarget;
        let startIndexForInsertion = textarea.selectionStart;
        let endIndexForInsertion = textarea.selectionEnd;
        let leftSide = textarea.value.substring(0, startIndexForInsertion) + '\t';
        let rightSide = textarea.value.substr(endIndexForInsertion);
        let textareaContent = leftSide + rightSide;
        textarea.value = textareaContent;
        let newSelectionRange = startIndexForInsertion + 1;
        textarea.setSelectionRange(newSelectionRange, newSelectionRange);
        e.preventDefault();
    }
}