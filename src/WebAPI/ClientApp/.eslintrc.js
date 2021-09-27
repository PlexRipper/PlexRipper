module.exports = {
	root: true,
	env: {
		browser: true,
		node: true,
	},
	extends: [
		'@nuxtjs/eslint-config-typescript',
		'prettier',
		'plugin:prettier/recommended',
		'plugin:nuxt/recommended',
		'plugin:@intlify/vue-i18n/recommended',
	],
	plugins: ['prettier'],
	// add your custom rules here
	rules: {
		'vue/valid-v-slot': 0,
		'vue/no-v-html': 'off',
		'no-use-before-define': 'off',
		'no-extend-native': 'off',
		'@intlify/vue-i18n/no-raw-text': [
			'warn',
			{
				ignoreNodes: ['md-icon', 'v-icon'],
				ignorePattern: '^[-#:()&]+$',
				ignoreText: ['EUR', 'HKD', 'USD', '%', '?', ''],
			},
		],
	},
	settings: {
		'vue-i18n': {
			localeDir: './src/lang/*.{json,json5,yaml,yml}', // extension is glob formatting!
			// Specify the version of `vue-i18n` you are using.
			// If not specified, the message will be parsed twice.
			messageSyntaxVersion: '^9.0.0',
		},
	},
};
