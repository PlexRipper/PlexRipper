import { NuxtConfig } from '@nuxt/types/config';
import { NuxtWebpackEnv } from '@nuxt/types/config/build';
import { Configuration as WebpackConfiguration } from 'webpack';
import TsconfigPathsPlugin from 'tsconfig-paths-webpack-plugin';

const config: NuxtConfig = {
	ssr: false,
	target: 'static',
	srcDir: 'src/',
	publicRuntimeConfig: {
		nodeEnv: process.env.NODE_ENV || 'development',
		version: process.env.npm_package_version || '?',
	},
	/*
	 ** Headers of the page
	 */
	head: {
		titleTemplate: 'PlexRipper',
		title: process.env.npm_package_name || '',
		meta: [
			{ charset: 'utf-8' },
			{ name: 'viewport', content: 'width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no, minimal-ui' },
			{
				hid: 'description',
				name: 'description',
				content: process.env.npm_package_description || '',
			},
		],
		link: [{ rel: 'icon', type: 'image/x-icon', href: '/favicon.png' }],
	},

	// Global CSS: https://go.nuxtjs.dev/config-css
	css: ['@/assets/scss/style.scss'],

	/*
	 ** Customize the progress-bar color
	 */
	loading: false,
	/*
	 ** Auto-import components
	 *  Doc: https://github.com/nuxt/components
	 */
	components: [
		// Components
		{
			path: '~/components',
			pathPrefix: false,
			extensions: ['vue'],
		},
		// Pages
		{
			path: '~/pages',
			pattern: '**/**/components/*.vue',
			pathPrefix: false,
			extensions: ['vue'],
		},
	],
	/*
	 ** Plugins to load before mounting the App
	 */
	plugins: [
		{ src: '@plugins/setup.ts', mode: 'client' },
		{ src: '@plugins/vuetify.ts', mode: 'client' },
		{ src: '@plugins/filters.ts', mode: 'client' },
		{ src: '@plugins/axios.ts', mode: 'client' },
		{ src: '@plugins/i18nPlugin.ts', mode: 'client' },
		{ src: '@plugins/registerPlugins.ts', mode: 'client' },
		{ src: '@plugins/registerComponents.ts', mode: 'client' },
		{ src: '@plugins/typeExtensions.ts', mode: 'client' },
	],
	router: {
		middleware: ['pageRedirect'],
		extendRoutes(routes, resolve) {
			routes.push({
				name: 'details-overview',
				path: '/tvshows/:id',
				component: resolve(__dirname, 'src/pages/tvshows/_id.vue'),
				children: [
					{
						path: 'details/:mediaid',
						component: resolve(__dirname, 'src/pages/tvshows/_id.vue'),
					},
				],
			});
		},
	},
	/*
	 ** Nuxt.js dev-modules
	 */
	buildModules: [
		// Doc: https://github.com/nuxt-community/eslint-module
		'@nuxtjs/eslint-module',
		// Doc: https://typescript.nuxtjs.org/guide/
		'@nuxt/typescript-build',
		// Doc: https://github.com/nuxt-community/stylelint-module
		'@nuxtjs/stylelint-module',
		// Doc: https://github.com/nuxt-community/vuetify-module
		'@nuxtjs/vuetify',
	],
	/*
	 ** Nuxt.js modules
	 */
	modules: [
		// Doc: https://axios.nuxtjs.org/usage
		'@nuxtjs/axios',
		// Doc: https://i18n.nuxtjs.org/
		'nuxt-i18n',
	],
	i18n: {
		lazy: true,
		langDir: 'lang/',
		locales: [{ code: 'en', iso: 'en-US', file: 'en-US.json' }],
		defaultLocale: 'en',
	},
	/*
	 ** Build configuration
	 */
	build: {
		/*
		 ** You can extend webpack config here
		 */
		hotMiddleware: {
			client: {
				overlay: false,
			},
		},
		extractCSS: true,
		// Will allow for debugging in Typescript + Nuxt
		// Doc: https://nordschool.com/enable-vs-code-debugger-for-nuxt-and-typescript/
		extend(config: WebpackConfiguration, { isDev, isClient }: NuxtWebpackEnv): void {
			if (isDev) {
				config.devtool = isClient ? 'source-map' : 'inline-source-map';
			}

			// Doc: https://github.com/dividab/tsconfig-paths-webpack-plugin
			config.resolve?.plugins?.push(new TsconfigPathsPlugin());
		},
	},
};

export default config;
