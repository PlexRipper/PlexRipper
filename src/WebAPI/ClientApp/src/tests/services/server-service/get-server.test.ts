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

	test('Should return a server by Id when given a serverId', async () => {
		// Arrange
		config = {
			seed: 1536,
			plexServerCount: 3,
		};
		const servers = generatePlexServers(config);
		mock.onGet(PLEX_SERVER_RELATIVE_PATH).reply(200, generateResultDTO(servers));
		const setup$ = ServerService.setup(ctx);
		const serverResult$ = ServerService.getServer(servers[2].id).pipe(take(1));

		// Act
		const setupResult = subscribeSpyTo(setup$);
		await setupResult.onComplete();
		const serverResult = subscribeSpyTo(serverResult$);
		await serverResult.onComplete();

		// Assert
		expect(serverResult.receivedComplete()).toBe(true);
		const firstValue = serverResult.getFirstValue();
		expect(firstValue).not.toBeNaN();
		expect(firstValue?.id).toEqual(3);
		expect(firstValue).toEqual(servers[2]);
	});
});
