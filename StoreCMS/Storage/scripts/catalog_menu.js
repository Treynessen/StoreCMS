// Не используй стрелочные функции и неопределенное количество параметров в функциях. Дважды ебаный IE не умеет с этим работать
function createDropMenuEvent(button, eventStarterIds){
	let dropped = false;
	let ulElement = button.getElementsByTagName('ul')[0];
	let cursorIntoMenuArea = false;
	button.addEventListener('mouseover', function() { cursorIntoMenuArea  = true; });
	button.addEventListener('mouseout', function() { cursorIntoMenuArea = false; });
	ulElement.addEventListener('mouseover', function() { cursorIntoMenuArea = true; });
	ulElement.addEventListener('mouseout', function() { cursorIntoMenuArea = false; });
	document.addEventListener('click', function() { if(dropped && !cursorIntoMenuArea) { button.classList.toggle('clicked'); dropped = false; } });
	return function(e) { 
		let canProcess = false;
		for (let i in eventStarterIds){
			if (eventStarterIds[i] == e.target.id){
				canProcess  = true;
				break;
			}
		}
		if (e.currentTarget.id == e.target.id)
			canProcess = true;
		if (canProcess){
			button.classList.toggle('clicked'); 
			dropped = !dropped; 
		}
	};
}
let catalogBtn = document.getElementById('catalog-button');
catalogBtn.addEventListener('click', createDropMenuEvent(catalogBtn, ['button', 'down-triangle']));