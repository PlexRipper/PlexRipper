import { fileURLToPath } from 'url';
import { resolve } from 'path';
import { defineNuxtConfig } from 'nuxt/config';
import { createCommonJS } from 'mlly';

const { __dirname } = createCommonJS(import.meta.url);

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
	ssr: false,
	srcDir: 'src',
	devtools: { enabled: true },
	runtimeConfig: {
		// Config within public will be also exposed to the client
		public: {
			nodeEnv: process.env.NODE_ENV || 'development',
			version: process.env.npm_package_version || '?',
			apiPort: process.env.API_PORT || '5000',
			isDocker: process.env.IS_DOCKER === 'true' || false,
		},
	},

	app: {
		head: {
			script: [
				{ src: 'https://cdnjs.cloudflare.com/ajax/libs/three.js/r134/three.min.js' },
				{ src: 'https://cdn.jsdelivr.net/npm/vanta/dist/vanta.waves.min.js' },
			],
			noscript: [{ children: 'JavaScript is required' }],
		},
	},

	modules: [
		// Doc: https://github.com/Maiquu/nuxt-quasar
		'nuxt-quasar-ui', // Doc: https://primevue.org/nuxt/
		'@vueuse/nuxt', // Doc: https://i18n.nuxtjs.org/
		'@primevue/nuxt-module',
		'@nuxtjs/i18n',
		'nuxt-lodash',
		'@nuxt/test-utils/module',
		[
			'@pinia/nuxt',
			{
				autoImports: ['defineStore', 'acceptHMRUpdate'],
			},
		],
		'@nuxt/eslint',
	],

	quasar: {
		// Plugins: https://quasar.dev/quasar-plugins
		// Truthy values requires `sass@1.32.12`.
		sassVariables: 'src/assets/scss/_variables.scss',
		iconSet: 'mdi-v7',
		config: {
			dark: true, // or 'auto'
		}, // Requires `@quasar/extras` package
		extras: {
			// string | null: Auto-import roboto font. https://quasar.dev/style/typography#default-font
			font: 'roboto-font', // string[]: Auto-import webfont icons. Usage: https://quasar.dev/vue-components/icon#webfont-usage
			fontIcons: ['mdi-v7'], // string[]: Auto-import svg icon collections. Usage: https://quasar.dev/vue-components/icon#svg-usage
			svgIcons: [], // string[]: Auto-import animations from 'animate.css'. Usage: https://quasar.dev/options/animations#usage
			// string[]: Auto-import animations from 'animate.css'. Usage: https://quasar.dev/options/animations#usage
			animations: ['fadeInLeft', 'fadeInRight', 'fadeInUp', 'fadeInDown', 'fadeOutLeft'],
		},
	},

	primevue: {
		importTheme: { from: '@/assets/scss/primevue/plexripper-theme.ts' },
	},

	typescript: {
		// Doc: https://typescript.nuxtjs.org/guide/setup.html#configuration
		// Packages,  @types/node, vue-tsc and typescript are required
		strict: true,
	},

	lodash: {
		prefix: false,
		prefixSkip: false,
		upperAfterPrefix: false,
	},

	i18n: {
		lazy: true,
		langDir: './lang/',
		defaultLocale: 'en-US', // Ensure the SettingsStore is updated as well when changes are made here:
		locales: [
			{ text: 'English', code: 'en-US', iso: 'en-US', bcp47Code: 'en', file: 'en-US.json' },
			{
				text: 'Français',
				code: 'fr-FR',
				iso: 'fr-FR',
				bcp47Code: 'fr',
				file: 'fr-FR.json',
			},
			{
				text: 'Deutsch',
				code: 'de-DE',
				iso: 'de-DE',
				bcp47Code: 'de',
				file: 'de-DE.json',
			},
			{
				text: 'Polski',
				code: 'pl-PL',
				iso: 'pl-PL',
				bcp47Code: 'pl',
				file: 'pl-PL.json',
			},
		],
		vueI18n: './src/config/vueI18n.config.ts',
		strategy: 'no_prefix',
	},

	/*
	 ** Global CSS: https://nuxt.com/docs/api/configuration/nuxt-config#css
	 */
	css: ['@/assets/scss/style.scss'],

	imports: {
		dirs: ['store'],
	},

	alias: {
		// Doc: https://nuxt.com/docs/api/configuration/nuxt-config#alias
		'@class': fileURLToPath(new URL('./src/types/class/', import.meta.url)),
		'@dto': fileURLToPath(new URL('./src/types/api/generated/data-contracts.ts', import.meta.url)),
		'@api': fileURLToPath(new URL('./src/types/api/', import.meta.url)),
		'@api-urls': fileURLToPath(new URL('./src/types/api/api-paths.ts', import.meta.url)),
		'@const': fileURLToPath(new URL('./src/types/const/', import.meta.url)),
		'@composables': fileURLToPath(new URL('./src/composables/', import.meta.url)),
		'@props': fileURLToPath(new URL('./src/types/props/', import.meta.url)),
		'@fixtures': fileURLToPath(new URL('./cypress/fixtures/', import.meta.url)),
		'@services-test-base': fileURLToPath(new URL('./tests/_base/base.ts', import.meta.url)),
		'@store': fileURLToPath(new URL('./src/store/', import.meta.url)),
		'@enums': fileURLToPath(new URL('./src/types/enums/', import.meta.url)),
		'@mock': fileURLToPath(new URL('./src/mock-data/', import.meta.url)),
		'@factories': fileURLToPath(new URL('./src/mock-data/factories/', import.meta.url)),
		'@interfaces': fileURLToPath(new URL('./src/types/interfaces/', import.meta.url)),
		'@components': fileURLToPath(new URL('./src/components/', import.meta.url)),
	},

	/*
	 ** Auto-import components
	 *  Doc: https://github.com/nuxt/components
	 */
	components: {
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

	nitro: {
		prerender: {
			crawlLinks: true,
		},
	},

	build: {
		transpile: ['primevue'],
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
	eslint: {
		config: {
			stylistic: {
				indent: 'tab',
				semi: true,
				arrowParens: true,
				blockSpacing: true,
				braceStyle: '1tbs',
				commaDangle: 'always-multiline',
				quotes: 'single',
				quoteProps: 'as-needed',
			},
		},
	},
	/*
	 ** Doc: https://nuxtjs.org/docs/configuration-glossary/configuration-telemetry
	 */
	// loading: true, // TODO Maybe better to re-enable based on how it looks
	telemetry: false,
	compatibilityDate: '2024-07-25',
});
