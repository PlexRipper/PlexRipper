import { take } from 'rxjs/operators';
import { subscribeSpyTo, baseSetup, getAxiosMock, baseVars } from '@services-test-base';
import AccountService from '@service/accountService';
import ServerService from '@service/serverService';
import { generatePlexAccounts, generatePlexLibraries, generatePlexServers, generateResultDTO } from '@mock';
import { PLEX_ACCOUNT_RELATIVE_PATH, PLEX_SERVER_RELATIVE_PATH } from '@api-urls';

describe('AccountService.createPlexAccount()', () => {
	let { mock, config } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
	});

	test('Should refresh servers when plex account is created successfully', async () => {
		// Arrange
		config = {
			seed: 263,
			plexServerCount: 3,
		};
		const plexServers = generatePlexServers({ config });
		const plexLibraries = plexServers.flatMap((x) => generatePlexLibraries({ plexServerId: x.id, config }));
		const plexAccount = generatePlexAccounts({ plexServers, plexLibraries, config })[0];

		mock.onGet(PLEX_SERVER_RELATIVE_PATH)
			.replyOnce(200, generateResultDTO([]))
			.onGet(PLEX_SERVER_RELATIVE_PATH)
			.reply(200, generateResultDTO(plexServers));
		mock.onGet(PLEX_ACCOUNT_RELATIVE_PATH).reply(200, generateResultDTO([]));
		mock.onPost(PLEX_ACCOUNT_RELATIVE_PATH).reply(200, generateResultDTO(plexAccount));
		mock.onGet(PLEX_ACCOUNT_RELATIVE_PATH + `/${plexAccount.id}`).reply(200, generateResultDTO(plexAccount));

		const accountServiceSetup$ = AccountService.setup();
		const serverServiceSetup$ = ServerService.setup();
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
		expect(getServersResult.getLastValue()).toEqual(plexServers);
		expect(createAccountResult.getLastValue()).toEqual(plexAccount);
	});
});
