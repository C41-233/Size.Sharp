(function(global){
	
function Wait(time){
	return new Promise((success, fail) => {
		global.setTimeout(() => {
			success();
		}, time);
	});
}
	
function Time(){
	return new Date().getTime();
}
	
let tree = null;
	
let app = new Vue({
	el: "#app",
	data() {
		return {
			props: {
				isLeaf: 'isLeaf'
			},
			loaded: false,
			percent: 0,
			totalObject: R.TotalObject,
			totalValue: R.TotalValue,
			totalSize: 0
		};
	},
	methods: {
		load(node, resolve) {
			if(node.level === 0){
				return resolve([{
					label: tree.name,
					node: () => tree,
					isLeaf: false
				}]);
			}
			
			let array = [];
			
			let obj = node.data.node();
			
			for(let name in obj.children){
				let child = obj.children[name];
				if(child.totalSize === 0){
					continue;
				}
				let span = {
					label: name,
					node: () => child
				};
				
				span.isLeaf = true;
				for(let key in child.children){
					if(child.children[key].totalSize > 0){
						span.isLeaf = false;
						break;
					}
				}
				
				array.push(span);
			}
			
			array.sort((x, y) => y.node().totalSize - x.node().totalSize);
			resolve(array);
		},
		renderContent(h, node){
			let obj = node.data.node();
			return h("div", [
				h("span", {attrs: {'class': "path-name"}}, obj.name),
				h("span", {attrs: {'class': 'path-type'}}, obj.type),
				h("span", {attrs: {'class': 'path-size'}}, `${this.toSize(obj.size)} / ${this.toSize(obj.totalSize)}`)
			])
		},
		toSize(val){
			if(val === 0){
				return 0;
			}
			if(val < 1024){
				return val + "B";
			}
			val /= 1024;
			if(val < 1024){
				return val.toFixed(3) + "K";
			}
			val /= 1024;
			if(val < 1024){
				return val.toFixed(3) + "M";
			}
			val /= 1024;
			return val.toFixed(3) + "G";
		}
	}
});


function getNodeByPath(root, path){
	let tokens = path.split('.');

	let node = root;
	for(let token of tokens){
		let child = node.children[token];
		if(child){
			node = node.children[token];
		}
		else{
			child = CreateNode(token);
			node.children[token] = child;
			node = child;
		}
	}
	return node;
}

function CreateNode(name){
	let index = parseInt(name);
	if(!isNaN(index)){
		name = `[${index}]`;
	}
	return {
		name: name,
		children: {},
		size: 0,
		totalSize: 0
	}
}

(async function(){
	let total = G.length;
	let current = 0;
	let size = 0;

	tree = CreateNode("<root>");

	let lastTime = Time();
	for(let input of G){
		if(input.alias){
			let node = getNodeByPath(tree, input.path);
			let alias = getNodeByPath(tree, input.alias);
			node.type = alias.type;
			node.alias = alias;
			Object.defineProperty(node, "size", {
				get(){
					return alias.size;
				}
			});
			Object.defineProperty(node, "totalSize", {
				get(){
					return alias.totalSize;
				}
			});
			node.children = alias.children;
		}
		else{
			let node = getNodeByPath(tree, input.path);
			node.type = input.type;
			node.size += input.size;
			size += input.size;
		}
		
		current++;
		app.percent = parseInt(current * 100 / total);
		
		let thisTime = Time();
		if(thisTime - lastTime > 350){
			await Wait(0);
			lastTime = thisTime;
		}
	}

	function CalcSize(node){
		node.visited = true;
		node.totalSize += node.size;
		
		for(let key in node.children){
			let child = node.children[key];
			if(child.visited || child.alias){
				continue;
			}
			node.totalSize += CalcSize(child);
		}
		return node.totalSize;
	}

	CalcSize(tree);
	
	app.totalSize = size;
	app.loaded = true;
})();

})(window);


