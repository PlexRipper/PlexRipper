import { defineConfig } from 'cypress';

export default defineConfig({
	component: {
		devServer: {
			framework: 'nuxt',
			bundler: 'webpack',
		},
	},

	viewportWidth: 1920,
	viewportHeight: 1080,
	e2e: {
		baseUrl: 'http://localhost:3030',
	},
});
