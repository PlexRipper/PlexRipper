import { resolve } from 'path';
import { defineConfig } from 'cypress';
import vitePreprocessor from 'cypress-vite';

export default defineConfig({
	projectId: 'qo5tth',
	env: {
		BASE_URL: 'http://localhost:3030',
	},
	video: false,
	screenshotOnRunFailure: false,
	e2e: {
		viewportHeight: 1080,
		viewportWidth: 1920,
		setupNodeEvents(on) {
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
