import { Context } from '@nuxt/types';
import { expect, jest, test } from '@jest/globals';
import mockAxios from 'jest-mock-axios';
import { GlobalService } from '@service';
import ServerService from '~/service/serverService';
import { generatePlexServers, generateResultDTO, PLEX_SERVER_API_URL } from '@mock';

describe('refresh-servers()', () => {
	const OLD_ENV = process.env;

	beforeEach(() => {
		jest.resetModules();
		process.env = { ...OLD_ENV }; // Make a copy
	});

	afterAll(() => {
		process.env = OLD_ENV; // Restore old environment
	});

	test('increments counter value on click', () => {
		const ctx: Context = {
			$config: {
				nodeEnv: 'TESTING',
				version: '1.0',
			},
		} as Context;
		process.env.NODE_ENV = 'dev';
		process.client = true;
		GlobalService.setConfigReady(ctx.$config);
		GlobalService.setup(ctx);

		mockAxios.mockResponseFor(
			{
				url: PLEX_SERVER_API_URL,
				method: 'GET',
			},
			{ data: generateResultDTO(null, generatePlexServers()) },
		);

		ServerService.refreshPlexServers();
		ServerService.getServers().subscribe((servers) => {
			expect(servers.length).toBe(5);
		});
	});
});
