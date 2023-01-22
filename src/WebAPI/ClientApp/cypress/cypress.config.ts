import { defineConfig } from 'cypress';

// NOTE: This Cypress config is used for the E2E testing in Docker, not for Local testing
export default defineConfig({
	video: false,
	component: {
		viewportHeight: 1080,
		viewportWidth: 960,
		devServer: {
			framework: 'nuxt',
			bundler: 'webpack',
		},
	},

	e2e: {
		baseUrl: 'http://localhost:7000',
	},
});
