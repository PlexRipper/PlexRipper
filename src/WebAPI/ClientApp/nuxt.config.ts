// noinspection AllyPlainJsInspection

import Log from 'consola';
import { NuxtConfig } from '@nuxt/types/config';
import { NuxtWebpackEnv } from '@nuxt/types/config/build';
import { Configuration as WebpackConfiguration } from 'webpack';
import TsconfigPathsPlugin from 'tsconfig-paths-webpack-plugin';

const config: NuxtConfig = {
	target: 'static',
	// Should always be true, otherwise SPA won't work once statically generated
	ssr: false,
	srcDir: 'src/',
	publicRuntimeConfig: {
		nodeEnv: process.env.NODE_ENV || 'development',
		version: process.env.npm_package_version || '?',
		baseURL: process.env.BASE_URL || 'http://localhost:5000',
	},
	/*
	 ** Doc: https://nuxtjs.org/docs/configuration-glossary/configuration-telemetry
	 */
	telemetry: false,
	/*
	 ** Headers of the page
	 */
	head: {
		titleTemplate: 'PlexRipper',
		title: process.env.npm_package_name || '',
		meta: [
			{ charset: 'utf-8' },
			{
				name: 'viewport',
				content: 'width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no, minimal-ui',
			},
			{
				hid: 'description',
				name: 'description',
				content: process.env.npm_package_description || '',
			},
		],
		link: [{ rel: 'icon', type: 'image/x-icon', href: '/favicon.png' }],
	},
	/*
	 ** Global CSS: https://go.nuxtjs.dev/config-css
	 */
	css: ['@/assets/scss/style.scss'],
	/*
	 ** Customize the progress-bar color
	 */
	loading: false,
	/*
	 ** Plugins to load before mounting the App
	 */
	plugins: [
		{ src: '@plugins/setup.ts', mode: 'client' },
		{ src: '@plugins/vuetify.ts' },
		{ src: '@plugins/filters.ts' },
		{ src: '@plugins/axios.ts' },
		{ src: '@plugins/i18nPlugin.ts' },
		{ src: '@plugins/registerPlugins.ts', mode: 'client' },
		{ src: '@plugins/typeExtensions.ts' },
	],

	/*
	 ** Nuxt.js modules
	 */
	modules: [
		// Doc: https://i18n.nuxtjs.org/
		'@nuxtjs/i18n',
	],
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
		// Doc: https://github.com/nuxt/components
		// Note: this is added to fix the error "render function or template not defined in component: "
		'@nuxt/components',
		// Doc: https://vueuse.org/guide/index.html#installation
		'@vueuse/nuxt',
		// Doc: https://github.com/fumeapp/nuxt-storm
		['nuxt-storm', { alias: true }],
		// Doc: https://github.com/nuxt/postcss8#readme
		'@nuxt/postcss8',
	],
	/*
	 ** Auto-import components
	 *  Doc: https://github.com/nuxt/components
	 */
	components: {
		loader: true,
		dirs: [
			// Components directory
			{
				path: './components',
				pathPrefix: false,
				extensions: ['vue'],
			},
			// Pages directory
			{
				path: './pages',
				pathPrefix: false,
				extensions: ['vue'],
			},
		],
	},
	router: {
		extendRoutes(routes, resolve) {
			routes.push({
				name: 'details-overview',
				path: '/tvshows/:libraryId',
				component: resolve(__dirname, 'src/pages/tvshows/_id.vue'),
				children: [
					{
						path: 'details/:tvShowId',
						component: resolve(__dirname, 'src/pages/tvshows/_id.vue'),
					},
				],
			});
		},
	},
	i18n: {
		lazy: true,
		langDir: 'lang/',
		defaultLocale: 'en-US',
		locales: [
			{ text: 'English', code: 'en-US', iso: 'en-US', file: 'en-US.json' },
			{ text: 'Français', code: 'fr-FR', iso: 'fr-FR', file: 'fr-FR.json' },
			{ text: 'Deutsch', code: 'de-DE', iso: 'de-DE', file: 'de-DE.json' },
		],
		vueI18n: {
			fallbackLocale: 'en-US',
		},
		strategy: 'no_prefix',
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
		transpile: [
			// This is needed to extend Vuetify components in ~/components/Extensions
			'vuetify/lib',
		],
		extractCSS: true,
		// Will allow for debugging in Typescript + Nuxt
		// Doc: https://nordschool.com/enable-vs-code-debugger-for-nuxt-and-typescript/
		extend(config: WebpackConfiguration, { isDev, isClient }: NuxtWebpackEnv): void {
			if (isDev) {
				config.devtool = isClient ? 'source-map' : 'inline-source-map';
			}

			// Fix for many errors with "Can't import the named export 'bypassFilter' from non EcmaScript module (only default export is available)"
			// Link: https://github.com/vueuse/vueuse/issues/718#issuecomment-913319680
			config?.module?.rules.push({
				test: /\.mjs$/,
				include: /node_modules/,
				type: 'javascript/auto',
			});

			// Doc: https://github.com/dividab/tsconfig-paths-webpack-plugin
			if (config.resolve?.plugins) {
				config.resolve.plugins.push(new TsconfigPathsPlugin());
			} else {
				Log.fatal('Setting up TS Path aliases in nuxt.config.ts => config.resolve.plugins was undefined');
			}
		},
	},
};

export default config;
