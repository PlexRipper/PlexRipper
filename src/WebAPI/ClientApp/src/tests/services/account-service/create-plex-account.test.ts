import { describe, beforeAll, expect, test } from '@jest/globals';
import { take } from 'rxjs/operators';
import { subscribeSpyTo, baseSetup, getAxiosMock, baseVars } from '@services-test-base';
import { AccountService, ServerService } from '@service';
import { generatePlexAccounts, generatePlexServers, generateResultDTO } from '@mock';
import { PLEX_ACCOUNT_RELATIVE_PATH, PLEX_SERVER_RELATIVE_PATH } from '@api-urls';

describe('AccountService.createPlexAccount()', () => {
	let { ctx, mock, config } = baseVars();

	beforeAll(() => {
		const result = baseSetup();
		ctx = result.ctx;
	});

	beforeEach(() => {
		mock = getAxiosMock();
	});

	test('Should refresh servers when plex account is created successfully', async () => {
		// Arrange
		config = {
			seed: 263,
		};
		const plexAccount = generatePlexAccounts(config)[0];
		const servers = generatePlexServers(config);

		mock.onGet(PLEX_SERVER_RELATIVE_PATH)
			.replyOnce(200, generateResultDTO([]))
			.onGet(PLEX_SERVER_RELATIVE_PATH)
			.reply(200, generateResultDTO(servers));
		mock.onGet(PLEX_ACCOUNT_RELATIVE_PATH).reply(200, generateResultDTO([]));
		mock.onPost(PLEX_ACCOUNT_RELATIVE_PATH).reply(200, generateResultDTO(plexAccount));
		mock.onGet(PLEX_ACCOUNT_RELATIVE_PATH + `/${plexAccount.id}`).reply(200, generateResultDTO(plexAccount));

		const accountServiceSetup$ = AccountService.setup(ctx);
		const serverServiceSetup$ = ServerService.setup(ctx);
		const createAccount$ = AccountService.createPlexAccount(plexAccount);
		const getServers$ = ServerService.getServers().pipe(take(2));

		// Act
		await subscribeSpyTo(accountServiceSetup$).onComplete();
		await subscribeSpyTo(serverServiceSetup$).onComplete();
		const createAccountResult = subscribeSpyTo(createAccount$);
		const getServersResult = subscribeSpyTo(getServers$);
		await createAccountResult.onComplete();
		await getServersResult.onComplete();

		// Assert
		expect(getServersResult.receivedComplete()).toEqual(true);
		expect(createAccountResult.receivedComplete()).toEqual(true);
		expect(getServersResult.getFirstValue()).toEqual([]);
		expect(getServersResult.getLastValue()).toEqual(servers);
		expect(createAccountResult.getLastValue()).toEqual(plexAccount);
	});
});
