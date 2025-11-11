import { defineConfig } from 'vitest/config';
import { defineVitestProject } from '@nuxt/test-utils/config';
import tsconfigPaths from 'vite-tsconfig-paths';

export default defineConfig({
	plugins: [
		tsconfigPaths(),
	],
	test: {
		projects: [
			await defineVitestProject({
				test: {
					name: 'unit',
					include: ['tests/unit/**/**/*.{test,spec}.ts'],
					environment: 'nuxt',
				},
			}),
			await defineVitestProject({
				test: {
					name: 'nuxt',
					include: ['tests/nuxt/**/**/*.test.ts'],
					environment: 'nuxt',
				},
			}),
		],
	},
});
