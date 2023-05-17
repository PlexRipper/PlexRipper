import { take } from 'rxjs/operators';
import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { PLEX_SERVER_CONNECTION_RELATIVE_PATH, PLEX_SERVER_RELATIVE_PATH } from '@api-urls';
import { generatePlexServers, generateResultDTO } from '@mock';
import ServerService from '@service/serverService';

describe('ServerService.getServers()', () => {
	let { mock, config } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
	});

	test('Should return a server by Id when given a serverId', async () => {
		// Arrange
		config = {
			seed: 1536,
			plexServerCount: 3,
		};
		const servers = generatePlexServers({ config });
		mock.onGet(PLEX_SERVER_RELATIVE_PATH).reply(200, generateResultDTO(servers));
		mock.onGet(PLEX_SERVER_CONNECTION_RELATIVE_PATH).reply(200, generateResultDTO([]));
		const setup$ = ServerService.setup();
		const serverResult$ = ServerService.getServer(servers[2].id).pipe(take(1));

		// Act
		const setupResult = subscribeSpyTo(setup$);
		await setupResult.onComplete();
		const serverResult = subscribeSpyTo(serverResult$);
		await serverResult.onComplete();

		// Assert
		expect(serverResult.receivedComplete()).toEqual(true);
		const firstValue = serverResult.getFirstValue();
		expect(firstValue).not.toBeNaN();
		expect(firstValue?.id).toEqual(servers[2].id);
		expect(firstValue).toEqual(servers[2]);
	});
});
