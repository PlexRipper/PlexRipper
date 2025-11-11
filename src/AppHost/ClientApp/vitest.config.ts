import nuxtConfig from './nuxt.config';
import { defineConfig } from 'vitest/config';
import { defineVitestProject } from '@nuxt/test-utils/config';

export default defineConfig({
	resolve: {
		alias: {
			...nuxtConfig.alias,
		},
	},
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
