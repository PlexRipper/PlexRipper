import vue from '@vitejs/plugin-vue';

// Workaround to get Cypress Component Testing working with Nuxt 3
export default {
	plugins: [
		vue({
			template: {
				compilerOptions: {
					isCustomElement: (tag) => tag.includes('-'),
				},
			},
		}),
	],
};
