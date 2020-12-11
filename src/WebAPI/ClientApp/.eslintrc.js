module.exports = {
	root: true,
	env: {
		browser: true,
		node: true,
	},
	extends: [
		'@nuxtjs/eslint-config-typescript',
		'prettier',
		'prettier/vue',
		'plugin:prettier/recommended',
		'plugin:nuxt/recommended',
	],
	plugins: ['prettier'],
	// add your custom rules here
	rules: {
		'vue/valid-v-slot': 0,
		'vue/no-v-html': 'off',
		'no-use-before-define': 'off',
	},
};
