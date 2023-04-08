import { defineConfig } from 'cypress';

export default defineConfig({
	env: {
		BASE_URL: 'http://localhost:3030',
		API_URL: 'http://localhost:5000/api',
	},
	e2e: {
		viewportHeight: 1080,
		viewportWidth: 1920,
		setupNodeEvents(on, config) {},
	},
});
