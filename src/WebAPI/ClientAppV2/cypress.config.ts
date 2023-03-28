import { defineConfig } from 'cypress';
import { defineConfig as defineVite, InlineConfig, mergeConfig } from 'vite';
import { esbuildCommonjs, viteCommonjs } from '@originjs/vite-plugin-commonjs';
import { buildNuxt, loadNuxt } from '@nuxt/kit';

export default defineConfig({
	component: {
		viewportHeight: 1080,
		viewportWidth: 1920,
		devServer: {
			framework: 'vue',
			bundler: 'vite',
			viteConfig: async () => {
				// Source: https://github.com/nuxt/nuxt/discussions/19304
				const config = await mergeConfig(viteConfig, await nuxtViteConfig());

				// @ts-ignore
				config.plugins = config.plugins?.filter((item) => !['replace', 'vite-plugin-eslint'].includes(item.name));

				// @ts-ignore
				config.server.middlewareMode = false;

				// eslint-disable-next-line no-console
				console.log('log:entering viteconfig', config);
				return config;
			},
		},
	},

	e2e: {
		viewportHeight: 1080,
		viewportWidth: 1920,
		setupNodeEvents(on, config) {
			// implement node event listeners here
		},
	},
});

const viteConfig = defineVite({
	optimizeDeps: {
		disabled: true,
		esbuildOptions: {
			plugins: [esbuildCommonjs(['consola', 'date-fns'])],
		},
	},
	plugins: [viteCommonjs()],
});

// https://github.com/nuxt/framework/issues/6496
async function nuxtViteConfig() {
	const nuxt = await loadNuxt({
		dev: false,
		overrides: {
			srcDir: 'src',
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
