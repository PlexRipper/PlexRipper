import { fileURLToPath } from 'url';
import { resolve } from 'path';
import { defineNuxtConfig } from 'nuxt/config';
import { createCommonJS } from 'mlly';

const { __dirname } = createCommonJS(import.meta.url);

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
	ssr: false,
	srcDir: 'src',
	devServer: {
		port: 3001,
	},
	runtimeConfig: {
		// Private config that is only available on the server
		apiSecret: '123',
		// Config within public will be also exposed to the client
		public: {
			nodeEnv: process.env.NODE_ENV || 'development',
			version: process.env.npm_package_version || '?',
			baseURL: process.env.BASE_URL || 'http://localhost:5000',
			baseApiPath: '/api',
		},
	},
	modules: [
		// Doc: https://github.com/Maiquu/nuxt-quasar
		'nuxt-quasar-ui',
		'@vueuse/nuxt',
		'@nuxt/devtools',
		// Doc: https://i18n.nuxtjs.org/
		'@nuxtjs/i18n',
		'@vue-macros/nuxt',
		'nuxt-vitest',
	],
	quasar: {
		// Plugins: https://quasar.dev/quasar-plugins
		plugins: ['Loading'],
		// Truthy values requires `sass@1.32.12`.
		sassVariables: 'src/assets/scss/_variables.scss',
		iconSet: 'mdi-v7',
		config: {
			dark: true, // or 'auto'
		},
		// Requires `@quasar/extras` package
		extras: {
			// string | null: Auto-import roboto font. https://quasar.dev/style/typography#default-font
			font: 'roboto-font',
			// string[]: Auto-import webfont icons. Usage: https://quasar.dev/vue-components/icon#webfont-usage
			fontIcons: ['mdi-v7'],
			// string[]: Auto-import svg icon collections. Usage: https://quasar.dev/vue-components/icon#svg-usage
			svgIcons: [],
			// string[]: Auto-import animations from 'animate.css'. Usage: https://quasar.dev/options/animations#usage
			animations: ['fadeInLeft', 'fadeInRight', 'fadeInUp', 'fadeInDown', 'fadeOutLeft'],
		},
	},
	typescript: {
		// Doc: https://typescript.nuxtjs.org/guide/setup.html#configuration
		// Packages,  @types/node, vue-tsc and typescript are required
		typeCheck: true,
		strict: true,
	},
	macros: {
		// Enabled betterDefine to allow importing interfaces into defineProps
		betterDefine: true,
		defineOptions: true,
	},
	i18n: {
		lazy: true,
		langDir: './lang/',
		defaultLocale: 'en-US',
		locales: [
			{ text: 'English', code: 'en-US', iso: 'en-US', file: 'en-US.json' },
			{ text: 'Français', code: 'fr-FR', iso: 'fr-FR', file: 'fr-FR.json' },
			{ text: 'Deutsch', code: 'de-DE', iso: 'de-DE', file: 'de-DE.json' },
		],
		vueI18n: './src/config/vueI18n.config.ts',
		strategy: 'no_prefix',
	},
	/*
	 ** Global CSS: https://nuxt.com/docs/api/configuration/nuxt-config#css
	 */
	css: ['@/assets/scss/style.scss'],
	alias: {
		// Doc: https://nuxt.com/docs/api/configuration/nuxt-config#alias
		'@class': fileURLToPath(new URL('./src/types/class/', import.meta.url)),
		'@dto': fileURLToPath(new URL('./src/types/dto/', import.meta.url)),
		'@api': fileURLToPath(new URL('./src/types/api/', import.meta.url)),
		'@const': fileURLToPath(new URL('./src/types/const/', import.meta.url)),
		'@buttons': fileURLToPath(new URL('./src/components/Buttons/', import.meta.url)),
		'@composables': fileURLToPath(new URL('./src/composables/', import.meta.url)),
		'@api-urls': fileURLToPath(new URL('./src/types/const/api-urls.ts', import.meta.url)),
		'@props': fileURLToPath(new URL('./src/types/props/', import.meta.url)),
		'@fixtures': fileURLToPath(new URL('./cypress/fixtures/', import.meta.url)),
		'@services-test-base': fileURLToPath(new URL('./tests/services/_base/base.ts', import.meta.url)),
		'@lib': fileURLToPath(new URL('./src/types/lib/', import.meta.url)),
		'@service': fileURLToPath(new URL('./src/service/', import.meta.url)),
		'@img': fileURLToPath(new URL('./src/assets/img/', import.meta.url)),
		'@enums': fileURLToPath(new URL('./src/types/enums/', import.meta.url)),
		'@mock': fileURLToPath(new URL('./src/mock-data/', import.meta.url)),
		'@factories': fileURLToPath(new URL('./src/mock-data/factories/', import.meta.url)),
		'@interfaces': fileURLToPath(new URL('./src/types/interfaces/', import.meta.url)),
		'@components': fileURLToPath(new URL('./src/components/', import.meta.url)),
		'@overviews': fileURLToPath(new URL('./src/components/Overviews/', import.meta.url)),
		'@mediaOverview': fileURLToPath(new URL('./src/components/MediaOverview/', import.meta.url)),
		'@vTreeViewTable': fileURLToPath(new URL('./src/components/General/VTreeViewTable/', import.meta.url)),
	},
	/*
	 ** Auto-import components
	 *  Doc: https://github.com/nuxt/components
	 */
	components: {
		loader: true,
		dirs: [
			// Components directory
			{
				path: '~/components',
				pathPrefix: false,
				extensions: ['vue'],
			},
		],
	},
	vite: {
		css: {
			preprocessorOptions: {
				scss: {
					// Make variables available everywhere
					// Doc: https://nuxt.com/docs/getting-started/assets#global-styles-imports
					additionalData: '@use "@/assets/scss/_variables.scss" as *;',
				},
			},
		},
	},
	hooks: {
		'pages:extend'(pages) {
			pages.push({
				name: 'media-overview',
				path: '/tvshows/:libraryId',
				file: resolve(__dirname, 'src/pages/tvshows/[libraryId].vue'),
				meta: {
					scrollPos: {
						top: 0,
						left: 0,
					},
				},
				children: [
					{
						name: 'details-overview',
						path: 'details/:tvShowId',
						file: resolve(__dirname, 'src/pages/tvshows/[libraryId].vue'),
						meta: {
							scrollPos: {
								top: 0,
								left: 0,
							},
						},
					},
				],
			});
		},
	},
	/*
	 ** Doc: https://nuxtjs.org/docs/configuration-glossary/configuration-telemetry
	 */
	telemetry: false,
	/*
	 ** Customize the progress-bar color
	 */
	// loading: true, // TODO Maybe better to re-enable based on how it looks
});
