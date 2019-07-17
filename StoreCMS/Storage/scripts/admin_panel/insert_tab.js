function insertTabEventHandler(e) {
    if (e.keyCode == 9 && !e.shiftKey && !e.ctrlKey && !e.altKey) {
        let textarea = e.currentTarget;
        let indexForInsertion = textarea.selectionStart;
        let leftSide = textarea.value.substring(0, indexForInsertion) + '    ';
        let rightSide = textarea.value.substr(indexForInsertion);
        let textareaContent = leftSide + rightSide;
        textarea.value = textareaContent;
        textarea.setSelectionRange(indexForInsertion + 4, indexForInsertion + 4);
        e.preventDefault();
    }
}