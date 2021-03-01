let app = new Vue({
    el: "#app",
    data() {
        return {
            props: {
                label: 'name',
                children: 'zones',
                isLeaf: 'leaf'
            },
        };
    },
    methods: {
        loadNode(node, resolve) {
            if (node.level === 0) {
                return resolve([{ name: 'region' }]);
            }
            if (node.level > 1) return resolve([]);

            setTimeout(() => {
                const data = [{
                    name: 'leaf',
                    leaf: true
                }, {
                    name: 'zone'
                }];

                resolve(data);
            }, 500);
        }
    }
});

