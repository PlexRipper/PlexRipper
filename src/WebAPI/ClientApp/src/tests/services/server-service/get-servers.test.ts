import Log from 'consola';
import { describe, beforeAll, expect, test } from '@jest/globals';
import { take } from 'rxjs/operators';
import { of } from 'rxjs';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { DOWNLOAD_RELATIVE_PATH, PLEX_SERVER_RELATIVE_PATH } from '@api-urls';
import { generatePlexServers, generateResultDTO } from '@mock';
import { ServerService } from '@service';
import ISetupResult from '@interfaces/service/ISetupResult';

describe('ServerService.getServers()', () => {
	let { ctx, mock, config } = baseVars();

	beforeAll(() => {
		const result = baseSetup();
		ctx = result.ctx;
	});

	beforeEach(() => {
		mock = getAxiosMock();
	});

	test('Should return success and complete when setup is run', async () => {
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
});
