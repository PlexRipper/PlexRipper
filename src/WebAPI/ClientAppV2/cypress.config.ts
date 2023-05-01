import { defineConfig } from 'cypress';

export default defineConfig({
	env: {
		BASE_URL: 'http://localhost:3030',
	},
	e2e: {
		viewportHeight: 1080,
		viewportWidth: 1920,
		setupNodeEvents(on, config) {},
	},
});
