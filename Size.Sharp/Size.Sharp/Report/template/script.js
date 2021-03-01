(function(){
	
let tree = CreateNode("/");

for(let input of G){
	let path = input.path.split('.');
	let node = tree;
	for(let token of path){
		if(node.children[token]){
			node = node.children[token];
		}
		else{
			let child = node.children[token] = CreateNode(token);
			child.parents[node.name] = node;
			node = child;
		}
	}
	node.type = input.type;
	node.size += input.size;
}

function CreateNode(name){
	let index = parseInt(name);
	if(!isNaN(index)){
		name = `[${index}]`;
	}
	return {
		name: name,
		children: {},
		parents: {},
		size: 0,
		totalSize: 0
	}
}

function CalcSize(node){
	node.visited = true;
	node.totalSize += node.size;
	
	for(let key in node.children){
		let child = node.children[key];
		if(child.visited){
			continue;
		}
		node.totalSize += CalcSize(child);
	}
	return node.totalSize;
}

CalcSize(tree);

let app = new Vue({
	el: "#app",
	data() {
		return {
			props: {
				isLeaf: 'isLeaf'
			}
		};
	},
	methods: {
		load(node, resolve) {
			if(node.level === 0){
				return resolve([{
					label: tree.name,
					node: tree,
					isLeaf: false
				}]);
			}
			
			let array = [];
			
			let obj = node.data.node;
			
			for(let name in obj.children){
				let span = {
					label: name,
					node: obj.children[name]
				};
				span.isLeaf = Object.keys(span.node.children).length === 0;
				array.push(span);
			}
			
			resolve(array);
		},
		renderContent(h, node){
			let obj = node.data.node;
			return h("div", [
				h("span", {attrs: {'class': "path-name"}}, obj.name),
				h("span", {attrs: {'class': 'path-type'}}, obj.type),
				h("span", {attrs: {'class': 'path-size'}}, `${obj.size} / ${obj.totalSize}`)
			])
		}
	}
});

})();


