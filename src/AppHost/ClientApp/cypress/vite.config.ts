import { defineConfig } from 'vite';
import tsconfigPaths from 'vite-tsconfig-paths';

export default defineConfig({
	// This is used otherwise path aliases during Cypress E2E tests will not work.
	plugins: [tsconfigPaths()],
});
