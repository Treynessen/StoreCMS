// Не используй стрелочные функции, for...of, includes, startsWith. Ебаный IE не умеет с ними работать Х_Х
let imageContainer = document.getElementById('image-container');
let imageBlocks = [];
let allBlocks = imageContainer.getElementsByTagName('div');
for (let i = 0, arrIndex = 0; i < allBlocks.length; ++i){
	if (allBlocks[i].id.indexOf('image-') == 0)
		imageBlocks[arrIndex++] = allBlocks[i];
}
let selectedImage = document.getElementById('selected-image');
let selectedImageBlock = selectedImage.parentElement;
let selectedBlock = imageBlocks[0];
selectedBlock.classList.toggle('selected');
selectedImage.src = selectedBlock.getElementsByTagName('img')[0].src;
let regex = /(_(\d)+x(\d)+)?(_q\d{1,3})?.jpg/;
selectedImageBlock.href = selectedImage.src.replace(regex, '.jpg');
if(allBlocks.length < 2){
	imageContainer.style.display = 'none';
}
else{
	let timer;
	let setImage = function(imageBlock) { 
		selectedImage.src = imageBlock.getElementsByTagName('img')[0].src; 
		selectedBlock.classList.toggle('selected');
		selectedBlock = imageBlock;
		selectedBlock.classList.toggle('selected');
		selectedImageBlock.href = selectedImage.src.replace(regex, '.jpg');
	};
	imageBlocks.forEach(function(value, index, arr){
		value.addEventListener('mouseover', function() { 
			timer = setTimeout(function() { setImage(value); }, 150);
		});
		value.addEventListener('mouseout', function() { clearTimeout(timer); });
	});
}