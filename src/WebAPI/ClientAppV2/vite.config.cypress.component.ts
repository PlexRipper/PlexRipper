import vue from '@vitejs/plugin-vue';
import { loadNuxt, buildNuxt } from '@nuxt/kit';
import type { InlineConfig } from 'vite';
import AutoImport from 'unplugin-auto-import/vite';
import { defineConfig as defineVite, mergeConfig } from 'vite';

// Workaround to get Cypress Component Testing working with Nuxt 3

const viteConfig = defineVite({
	optimizeDeps: {
		disabled: true,
		include: ['cypress/vue', 'consola'],
	},
	plugins: [
		// vue({
		// 	reactivityTransform: true,
		// 	template: {
		// 		compilerOptions: {
		// 			ssr: false,
		// 			isCustomElement: (tag) => true,
		// 		},
		// 	},
		// }),
		// AutoImport({
		// 	imports: [
		// 		'quasar',
		// 		'vue/macros',
		// 		{
		// 			consola: ['default', 'consola'],
		// 		},
		// 	],
		// 	vueTemplate: true,
		// }),
	],
});

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

export async function getViteConfig(): Promise<InlineConfig> {
	return mergeConfig(viteConfig, await nuxtViteConfig());
}
