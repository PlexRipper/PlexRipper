import { take } from 'rxjs/operators';
import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { PLEX_SERVER_RELATIVE_PATH } from '@api-urls';
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

	test('Should return all servers when servers are set in the store', async () => {
		// Arrange
		config = {
			plexServerCount: 3,
		};
		const servers = generatePlexServers({ config });
		mock.onGet(PLEX_SERVER_RELATIVE_PATH).reply(200, generateResultDTO(servers));
		const setup$ = ServerService.setup();
		const serversResult$ = ServerService.getServers().pipe(take(1));

		// Act
		const setupResult = subscribeSpyTo(setup$);
		await setupResult.onComplete();
		const serversResult = subscribeSpyTo(serversResult$);
		await serversResult.onComplete();

		// Assert
		expect(serversResult.receivedComplete()).toEqual(true);
		expect(serversResult.getFirstValue()).toEqual(servers);
	});

	test('Should return all servers by id when servers are set in the store', async () => {
		// Arrange
		config = {
			seed: 4587,
			plexServerCount: 6,
		};
		const servers = generatePlexServers({ config });
		const serverIds = servers.map((x) => x.id);
		mock.onGet(PLEX_SERVER_RELATIVE_PATH).reply(200, generateResultDTO(servers));
		const setup$ = ServerService.setup();
		const serversResult$ = ServerService.getServers(serverIds.slice(0, 3)).pipe(take(1));

		// Act
		const setupResult = subscribeSpyTo(setup$);
		await setupResult.onComplete();
		const serversResult = subscribeSpyTo(serversResult$);
		await serversResult.onComplete();

		// Assert
		expect(serversResult.receivedComplete()).toEqual(true);
		const firstValue = serversResult.getFirstValue();
		expect(firstValue.length).toEqual(3);
		for (let i = 0; i < 3; i++) {
			expect(firstValue[i].id).toEqual(serverIds[i]);
		}
	});
});
