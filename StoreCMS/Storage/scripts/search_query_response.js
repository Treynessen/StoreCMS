let products = document.querySelectorAll('div.content-block > a.product');
if(products.length == 0){
	let info = document.createElement('p');
	let regex = /search=.[^&]*/i;
	let searchQuery = regex.exec(location.search);
	if(searchQuery !== null){
		searchQuery = decodeURIComponent(searchQuery.toString()).substr(7);
	}
	info.textContent = 'По запросу ' + searchQuery + ' ничего не найдено';
	let contentBlock = document.querySelector('div.content-block');
	contentBlock.appendChild(info);
}