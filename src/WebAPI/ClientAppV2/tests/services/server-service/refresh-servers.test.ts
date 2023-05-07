import { baseVars, subscribeSpyTo, baseSetup, getAxiosMock } from '@services-test-base';
import { generatePlexServers, generateResultDTO } from '@mock';
import { PLEX_SERVER_RELATIVE_PATH } from '@api-urls';
import ServerService from '@service/serverService';

describe('ServerService.refresh-servers()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
	});

	test('Should update the plexServers when refreshPlexServers is called', async () => {
		// Arrange
		const servers = generatePlexServers({
			config: {
				plexServerCount: 3,
			},
		});
		mock.onGet(PLEX_SERVER_RELATIVE_PATH)
			.replyOnce(200, [])
			.onGet(PLEX_SERVER_RELATIVE_PATH)
			.reply(200, generateResultDTO(servers));
		const setup$ = ServerService.setup();
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
