module.exports = {
	root: true,
	env: {
		browser: true,
		node: true,
	},
	extends: [
		'plugin:@intlify/vue-i18n/recommended',
		'@nuxtjs/eslint-config-typescript',
		'plugin:nuxt/recommended',
		'prettier',
		'plugin:prettier/recommended',
	],
	plugins: ['prettier'],
	// add your custom rules here
	rules: {
		// 'vue/html-quotes': ['error', 'double', { avoidEscape: false }],
		'vue/valid-v-slot': 0,
		'vue/no-v-html': 'off',
		'no-use-before-define': 'off',
		'no-extend-native': 'off',
		'import/no-named-as-default': 'off',
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
