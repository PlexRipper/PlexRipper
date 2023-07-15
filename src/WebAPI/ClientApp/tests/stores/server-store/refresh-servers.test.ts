import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseVars, subscribeSpyTo, baseSetup, getAxiosMock } from '@services-test-base';
import { generatePlexServers, generateResultDTO } from '@mock';
import { PLEX_SERVER_RELATIVE_PATH } from '@api-urls';
import { useServerStore } from '#imports';

describe('ServerStore.refresh-servers()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should update the plexServers when refreshPlexServers is called', async () => {
		// Arrange
		const serverStore = useServerStore();
		const servers = generatePlexServers({
			config: {
				plexServerCount: 3,
			},
		});
		mock.onGet(PLEX_SERVER_RELATIVE_PATH)
			.replyOnce(200, [])
			.onGet(PLEX_SERVER_RELATIVE_PATH)
			.reply(200, generateResultDTO(servers));

		// Act
		await subscribeSpyTo(serverStore.setup()).onComplete();
		const refreshResult = subscribeSpyTo(serverStore.refreshPlexServers());
		await refreshResult.onComplete();
		// Assert
		expect(refreshResult.getFirstValue()).toEqual(servers);
	});
});
