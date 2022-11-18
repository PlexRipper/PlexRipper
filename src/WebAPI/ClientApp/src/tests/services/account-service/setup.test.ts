import { describe, beforeAll, expect, test } from '@jest/globals';
import { subscribeSpyTo, baseSetup, getAxiosMock, baseVars } from '@services-test-base';
import { AccountService, GlobalService } from '@service';
import { generateResultDTO } from '@mock';
import { PLEX_ACCOUNT_RELATIVE_PATH } from '@api-urls';
import ISetupResult from '@interfaces/service/ISetupResult';

describe('AccountService.setup()', () => {
	let { ctx, mock } = baseVars();

	beforeAll(() => {
		const result = baseSetup();
		ctx = result.ctx;
	});

	beforeEach(() => {
		mock = getAxiosMock();
		GlobalService.initializeState();
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		mock.onGet(PLEX_ACCOUNT_RELATIVE_PATH).reply(200, generateResultDTO([]));
		const setup$ = AccountService.setup(ctx);
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: AccountService.name,
		};

		// Act
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toBe(true);
	});
});
