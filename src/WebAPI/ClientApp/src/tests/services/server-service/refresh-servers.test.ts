import { beforeAll, expect, test } from '@jest/globals';
import { baseVars, beforeEachServiceTest, subscribeSpyTo, baseSetup, getAxiosMock } from '@services-test-base';
import Log from 'consola';
import { generatePlexServers, generateResultDTO } from '@mock';
import { PLEX_SERVER_RELATIVE_PATH } from '@api-urls';
import { GlobalService, ServerService } from '@service';

describe('ServerService.refresh-servers()', () => {
	let { ctx, mock, config } = baseVars();

	beforeAll(() => {
		const result = baseSetup();
		ctx = result.ctx;
		mock = getAxiosMock();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		GlobalService.initializeState();
	});

	test('Should update the plexServers when refreshPlexServers is called', async () => {
		// Arrange
		config = {
			plexServerCount: 3,
		};
		const servers = generatePlexServers(config);
		mock.onGet(PLEX_SERVER_RELATIVE_PATH)
			.replyOnce(200, [])
			.onGet(PLEX_SERVER_RELATIVE_PATH)
			.reply(200, generateResultDTO(servers));
		const setup$ = ServerService.setup(ctx);
		const refresh$ = ServerService.refreshPlexServers();
		const getServers$ = ServerService.getServers();

		// Act
		subscribeSpyTo(getServers$);
		const setupResult = subscribeSpyTo(setup$);
		await setupResult.onComplete();
		const refreshResult = subscribeSpyTo(refresh$);
		await refreshResult.onComplete();
		// Assert
		expect(refreshResult.getFirstValue()).toEqual(servers);
	});
});
