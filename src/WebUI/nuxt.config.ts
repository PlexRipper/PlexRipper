import * as path from 'path';
import { NuxtConfig } from '@nuxt/types/config';
import { NuxtWebpackEnv } from '@nuxt/types/config/build';
import { Configuration as WebpackConfiguration } from 'webpack';

const config: NuxtConfig = {
	mode: 'spa',
	target: 'static',
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
	 ** Customize the progress-bar color
	 */
	loading: { color: '#fff' },
	/*
	 ** Global CSS
	 */
	css: [],
	/*
	 ** Plugins to load before mounting the App
	 */
	plugins: [
		{ src: '@plugins/consola.ts', mode: 'client' },
		{ src: '@plugins/filters.ts', mode: 'client' },
		{ src: '@plugins/axios.ts', mode: 'client' },
		{ src: '@plugins/perfect-scrollbar.ts', mode: 'client' },
	],
	/*
	 ** Nuxt.js dev-modules
	 */
	buildModules: [
		// Doc: https://typescript.nuxtjs.org/guide/
		'@nuxt/typescript-build',
		// Doc: https://github.com/nuxt-community/stylelint-module
		'@nuxtjs/stylelint-module',
		[
			'@nuxtjs/vuetify',
			{
				customVariables: ['~/assets/scss/_variables.scss'],
				theme: {
					options: {
						customProperties: true,
					},
					dark: true,
				},
			},
		],
	],
	/*
	 ** Nuxt.js modules
	 */
	modules: [
		// Doc: https://axios.nuxtjs.org/usage
		'@nuxtjs/axios',
		// Doc: https://github.com/nuxt-community/dotenv-module
		'@nuxtjs/dotenv',
	],
	/*
	 ** Build configuration
	 */
	build: {
		/*
		 ** You can extend webpack config here
		 */
		// Will allow for debugging in Typescript + Nuxt
		// Doc: https://nordschool.com/enable-vs-code-debugger-for-nuxt-and-typescript/
		extend(config: WebpackConfiguration, { isDev, isClient }: NuxtWebpackEnv): void {
			if (isDev) {
				config.devtool = isClient ? 'source-map' : 'inline-source-map';
			}

			// Make sure to also update the tsconfig.json when adding aliases for import resolving.
			// These are necessary to tell webpack which aliases are used.
			if (config && config.resolve && config.resolve.alias) {
				config.resolve.alias['@store'] = path.resolve(__dirname, 'src/store/');
				config.resolve.alias['@dto'] = path.resolve(__dirname, 'src/types/dto/');
				config.resolve.alias['@api'] = path.resolve(__dirname, 'src/types/api/');
				config.resolve.alias['@service'] = path.resolve(__dirname, 'src/types/service/');
				config.resolve.alias['@components'] = path.resolve(__dirname, 'src/components/');
				config.resolve.alias['@mediaOverview'] = path.resolve(__dirname, 'src/components/MediaOverview/');
			}
		},
	},
};

export default config;
