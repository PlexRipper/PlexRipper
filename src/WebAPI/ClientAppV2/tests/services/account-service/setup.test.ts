import { subscribeSpyTo, baseSetup, getAxiosMock, baseVars } from '@services-test-base';
import AccountService from '@service/accountService';
import { generateResultDTO } from '@mock';
import { PLEX_ACCOUNT_RELATIVE_PATH } from '@api-urls';
import ISetupResult from '@interfaces/service/ISetupResult';

describe('AccountService.setup()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		mock.onGet(PLEX_ACCOUNT_RELATIVE_PATH).reply(200, generateResultDTO([]));
		const setup$ = AccountService.setup();
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: AccountService.name,
		};

		// Act
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
