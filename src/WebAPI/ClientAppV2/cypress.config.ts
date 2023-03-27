import { defineConfig } from 'cypress';
import { getViteConfig } from './vite.config.cypress.component';

export default defineConfig({
	component: {
		viewportHeight: 1080,
		viewportWidth: 1920,
		devServer: {
			framework: 'vue',
			bundler: 'vite',
			viteConfig: async () => {
				// Source: https://github.com/nuxt/nuxt/discussions/19304
				const config = await getViteConfig();

				config.plugins = config.plugins.filter((item) => !['replace', 'vite-plugin-eslint'].includes(item.name));

				// @ts-ignore
				config.server.middlewareMode = false;

				// eslint-disable-next-line no-console
				console.log('log:entering viteconfig', config);
				return config;
			},
		},
	},
});
