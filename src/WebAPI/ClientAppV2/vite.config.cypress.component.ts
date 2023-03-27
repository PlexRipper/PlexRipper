import vue from '@vitejs/plugin-vue';
import { loadNuxt, buildNuxt } from '@nuxt/kit';
import type { InlineConfig } from 'vite';

// Workaround to get Cypress Component Testing working with Nuxt 3
const viteConfig = {
	optimizeDeps: {
		disabled: true,
	},
	plugins: [
		vue({
			template: {
				// preprocessOptions: {
				// 	scss: {
				// 		// Make variables available everywhere
				// 		// Doc: https://nuxt.com/docs/getting-started/assets#global-styles-imports
				// 		additionalData: '@use "@/assets/scss/_variables.scss" as *;',
				// 	},
				// },
				compilerOptions: {
					isCustomElement: (tag) => tag.includes('-'),
				},
			},
		}),
	],
};

// https://github.com/nuxt/framework/issues/6496
async function nuxtViteConfig() {
	const nuxt = await loadNuxt({
		dev: false,
		overrides: {
			ssr: false,
		},
	});
	return new Promise<InlineConfig>((resolve, reject) => {
		nuxt.hook('vite:extendConfig', (config) => {
			resolve(config);
			throw new Error('_stop_');
		});
		buildNuxt(nuxt).catch((err) => {
			if (!err.toString().includes('_stop_')) {
				reject(err);
			}
		});
	}).finally(() => nuxt.close());
}

export async function getViteConfig() {
	return {
		...Object.assign({}, await nuxtViteConfig()),
		...viteConfig,
	};
}
