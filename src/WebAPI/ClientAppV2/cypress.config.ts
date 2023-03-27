import { defineConfig } from 'cypress';
import viteConfig from './vite.config.cypress.component.js';

export default defineConfig({
	component: {
		viewportHeight: 1080,
		viewportWidth: 1920,
		devServer: {
			framework: 'vue',
			bundler: 'vite',
			viteConfig,
		},
	},
});
