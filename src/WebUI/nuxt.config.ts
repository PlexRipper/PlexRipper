import * as path from 'path';
import { NuxtConfig } from '@nuxt/types/config';
import { NuxtWebpackEnv } from '@nuxt/types/config/build';
import { Configuration as WebpackConfiguration } from 'webpack';

const config: NuxtConfig = {
	ssr: false,
	target: 'static',
	srcDir: 'src/',
	/*
	 ** Headers of the page
	 */
	head: {
		titleTemplate: 'PlexRipper',
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
		link: [{ rel: 'icon', type: 'image/x-icon', href: '/favicon.png' }],
	},
	/*
	 ** Customize the progress-bar color
	 */
	loading: { color: '#fff' },
	/*
	 ** Plugins to load before mounting the App
	 */
	plugins: [
		{ src: '@plugins/consola.ts', mode: 'client' },
		{ src: '@plugins/vuetify.ts', mode: 'client' },
		{ src: '@plugins/filters.ts', mode: 'client' },
		{ src: '@plugins/axios.ts', mode: 'client' },
		{ src: '@plugins/i18nPlugin.ts', mode: 'client' },
		{ src: '@plugins/registerPlugins.ts', mode: 'client' },
		{ src: '@plugins/registerComponents.ts', mode: 'client' },
	],
	router: {
		middleware: ['pageRedirect'],
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
		'@nuxtjs/style-resources',
		// Doc: https://axios.nuxtjs.org/usage
		'@nuxtjs/axios',
		// Doc: https://github.com/nuxt-community/dotenv-module
		'@nuxtjs/dotenv',
		// Doc: https://i18n.nuxtjs.org/
		'nuxt-i18n',
	],
	i18n: {
		lazy: true,
		langDir: '/lang/',
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
		// Will allow for debugging in Typescript + Nuxt
		// Doc: https://nordschool.com/enable-vs-code-debugger-for-nuxt-and-typescript/
		extend(config: WebpackConfiguration, { isDev, isClient }: NuxtWebpackEnv): void {
			// console.log(isDev + ' - ' + isClient);
			if (isDev) {
				config.devtool = isClient ? 'source-map' : 'inline-source-map';
			}

			// Make sure to also update the tsconfig.json when adding aliases for import resolving.
			// These are necessary to tell webpack which aliases are used.
			if (config && config.resolve && config.resolve.alias) {
				config.resolve.alias['@store'] = path.resolve(__dirname, 'src/store/');
				config.resolve.alias['@dto'] = path.resolve(__dirname, 'src/types/dto/');
				config.resolve.alias['@api'] = path.resolve(__dirname, 'src/types/api/');
				config.resolve.alias['@state'] = path.resolve(__dirname, 'src/types/state/');
				config.resolve.alias['@img'] = path.resolve(__dirname, 'src/assets/img/');
				config.resolve.alias['@enums'] = path.resolve(__dirname, 'src/types/enums/');
				config.resolve.alias['@interfaces'] = path.resolve(__dirname, 'src/types/interfaces/');
				config.resolve.alias['@service'] = path.resolve(__dirname, 'src/types/service/');
				config.resolve.alias['@components'] = path.resolve(__dirname, 'src/components/');
				config.resolve.alias['@components'] = path.resolve(__dirname, 'src/components/');
				config.resolve.alias['@overviews'] = path.resolve(__dirname, 'src/components/overviews');
				config.resolve.alias['@mediaOverview'] = path.resolve(__dirname, 'src/components/MediaOverview/');
			}
		},
	},
};

export default config;
