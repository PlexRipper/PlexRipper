import { defineConfig } from 'cypress';

export default defineConfig({
	e2e: {
		viewportHeight: 1080,
		viewportWidth: 1920,
		setupNodeEvents(on, config) {
			// implement node event listeners here
		},
	},
});
