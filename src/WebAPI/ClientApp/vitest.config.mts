import { defineVitestConfig } from '@nuxt/test-utils/config';

export default defineVitestConfig({
	mode: 'test',
	test: {
		globals: true,
		// We use happy-dom as the test environment because it allows us to test the DOM in Node.js
		environment: 'happy-dom',
		include: ['./tests/**/**/*.test.ts'],
	},
});
