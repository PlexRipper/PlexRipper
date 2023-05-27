import { resolve } from 'path';
import { defineConfig } from 'cypress';

export default defineConfig({
	env: {
		BASE_URL: 'http://localhost:3030',
	},
	video: false,
	screenshotOnRunFailure: false,
	e2e: {
		viewportHeight: 1080,
		viewportWidth: 1920,
		setupNodeEvents(on) {
			// Source: https://github.com/mammadataei/cypress-vite/issues/10
			// eslint-disable-next-line @typescript-eslint/no-var-requires
			const vitePreprocessor = require('cypress-vite');
			on(
				'file:preprocessor',
				vitePreprocessor({
					configFile: resolve(__dirname, './cypress/vite.config.ts'),
					mode: 'development',
				}),
			);
		},
	},
});
