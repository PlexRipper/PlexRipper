import { describe, beforeAll, expect, test } from '@jest/globals';
import { take } from 'rxjs/operators';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { PLEX_SERVER_RELATIVE_PATH } from '@api-urls';
import { generatePlexServers, generateResultDTO } from '@mock';
import { ServerService } from '@service';

describe('ServerService.getServers()', () => {
	let { ctx, mock, config } = baseVars();

	beforeAll(() => {
		const result = baseSetup();
		ctx = result.ctx;
	});

	beforeEach(() => {
		mock = getAxiosMock();
	});

	test('Should return all servers when servers are set in the store', async () => {
		// Arrange
		config = {
			plexServerCount: 3,
		};
		const servers = generatePlexServers(config);
		mock.onGet(PLEX_SERVER_RELATIVE_PATH).reply(200, generateResultDTO(servers));
		const setup$ = ServerService.setup(ctx);
		const serversResult$ = ServerService.getServers().pipe(take(1));

		// Act
		const setupResult = subscribeSpyTo(setup$);
		await setupResult.onComplete();
		const serversResult = subscribeSpyTo(serversResult$);
		await serversResult.onComplete();

		// Assert
		expect(serversResult.receivedComplete()).toBe(true);
		expect(serversResult.getFirstValue()).toEqual(servers);
	});

	test('Should return all servers by id when servers are set in the store', async () => {
		// Arrange
		config = {
			seed: 4587,
			plexServerCount: 6,
		};
		const servers = generatePlexServers(config);
		mock.onGet(PLEX_SERVER_RELATIVE_PATH).reply(200, generateResultDTO(servers));
		const setup$ = ServerService.setup(ctx);
		const serversResult$ = ServerService.getServers([2, 3, 4]).pipe(take(1));

		// Act
		const setupResult = subscribeSpyTo(setup$);
		await setupResult.onComplete();
		const serversResult = subscribeSpyTo(serversResult$);
		await serversResult.onComplete();

		// Assert
		expect(serversResult.receivedComplete()).toBe(true);
		const firstValue = serversResult.getFirstValue();
		expect(firstValue.length).toEqual(3);
		expect(firstValue[0].id).toEqual(2);
		expect(firstValue[1].id).toEqual(3);
		expect(firstValue[2].id).toEqual(4);
	});
});
