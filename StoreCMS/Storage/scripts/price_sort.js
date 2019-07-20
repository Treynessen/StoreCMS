let sortMenuBlock = document.querySelector('div.sort-panel > span > b.sort-menu');
let searchStr = location.search;
let aElement = document.createElement('a');
if (searchStr.length == 0){
	searchStr =  '?orderby=ascending_price';
	aElement.textContent = 'по возрастанию';
}
else {
	let regex = /orderby=[a-z]+_[a-z]+/i;
	let foundSubStr = regex.exec(searchStr);
	if (foundSubStr == null){
		foundSubStr = new String();
	}
	else foundSubStr = foundSubStr.toString();
	switch (foundSubStr.toLowerCase()){
		case 'orderby=ascending_price':
			searchStr = searchStr.replace(foundSubStr, 'orderby=descending_price');
			aElement.textContent = 'по убыванию';
			break;
		case 'orderby=descending_price':
			searchStr = searchStr.replace(foundSubStr, 'orderby=ascending_price');
			aElement.textContent = 'по возрастанию';
			break;
		case '':
			searchStr += '&orderby=ascending_price';
			aElement.textContent = 'по возрастанию';
			break;
		default:
			searchStr = searchStr.replace(foundSubStr, 'orderby=ascending_price');
			aElement.textContent = 'по возрастанию';
			break;
	}
}
aElement.setAttribute('href', location.origin + location.pathname + searchStr);
sortMenuBlock.appendChild(aElement);