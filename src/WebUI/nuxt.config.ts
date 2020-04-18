import { Configuration } from '@nuxt/types/config';
import * as path from 'path';

const config: Configuration = {
	mode: 'spa',
	srcDir: 'src/',
	/*
	 ** Headers of the page
	 */
	head: {
		titleTemplate: `%s - ${process.env.npm_package_name}`,
		title: process.env.npm_package_name || '',
		meta: [
			{ charset: 'utf-8' },
			{ name: 'viewport', content: 'width=device-width, initial-scale=1' },
			{
				hid: 'description',
				name: 'description',
				content: process.env.npm_package_description || '',
			},
		],
		link: [{ rel: 'icon', type: 'image/x-icon', href: '/favicon.ico' }],
	},
	/*
	 ** Plugins to load before mounting the App
	 */
	plugins: [
		{ src: '@plugins/vuetify.ts', mode: 'client' },
		{ src: '@plugins/loggingPlugin.ts', mode: 'client' },
	],
	/*
	 ** Nuxt.js dev-modules
	 */
	buildModules: [
		// Doc: https://typescript.nuxtjs.org/guide/
		[
			'@nuxt/typescript-build',
			{
				typeCheck: {
					memoryLimit: 2048,
					workers: 4,
				},
			},
		],
		// Doc: https://github.com/nuxt-community/eslint-module
		'@nuxtjs/eslint-module',
		// Doc: https://github.com/nuxt-community/stylelint-module
		'@nuxtjs/stylelint-module',
		'@nuxtjs/vuetify',
	],
	/*
	 ** Nuxt.js modules
	 */
	modules: [
		'@nuxtjs/proxy',
		// Doc: https://axios.nuxtjs.org/usage
		'@nuxtjs/axios',
		['@nuxtjs/pwa', { icon: false }],
		// Doc: https://github.com/nuxt-community/style-resources-module
		'@nuxtjs/style-resources',
		// Doc: https://github.com/nuxt-community/nuxt-i18n
		'nuxt-i18n',
	],
	/*
	 ** Nuxt Style Resources module configuration
	 ** https://github.com/nuxt-community/style-resources-module
	 */
	styleResources: {
		scss: [
			// WARNING: Do not import actual styles here. Use this module only to import variables, mixins,
			// functions (et cetera) as they won't exist in the actual build.
			// Importing actual styles will include them in every component and will also make your build/HMR magnitudes slower.

			// Global variables, site-wide settings, config switches, etc
			'@/assets/scss/_variables.scss',
			'@/assets/scss/_mixins.scss',
		],
	},
	/*
	 ** Build configuration
	 */
	build: {
		/*
		 ** You can extend webpack config here
		 */
		// Will allow for debugging in Typescript + Nuxt
		// Doc: https://nordschool.com/enable-vs-code-debugger-for-nuxt-and-typescript/
		extend(config, { isDev, isClient }): void {
			if (isDev) {
				config.devtool = isClient ? 'source-map' : 'inline-source-map';
			}

			if (!config || !config.module) {
				return;
			}
			// This will ignore any Markdown.md files
			// https://github.com/rails/webpacker/issues/1571
			config.module.rules.push({
				test: /\.md$/,
				loader: 'ignore-loader',
			});

			// Make sure to also update the tsconfig.json when adding aliases for import resolving.
			// These are necessary to tell webpack which aliases are used.
			if (config && config.resolve && config.resolve.alias) {
				config.resolve.alias['@'] = path.resolve(__dirname, 'src/');
				config.resolve.alias['@iGeneric'] = path.resolve(__dirname, 'src/types/dto/generic/');
			}
		},
		parallel: true,
		cache: true,
		babel: {
			presets: [
				'@vue/app',
				[
					'@babel/preset-env',
					{
						targets: {
							node: 'current',
						},
					},
				],
			],
			plugins: [['transform-imports'], '@babel/plugin-proposal-nullish-coalescing-operator', '@babel/plugin-proposal-optional-chaining'],
		},
	},
	transpileDependencies: ['vuex-persist'],
};

export default config;
