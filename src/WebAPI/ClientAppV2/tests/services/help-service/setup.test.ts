import { describe, beforeAll, expect, test } from '@jest/globals';
import { subscribeSpyTo, baseSetup, baseVars } from '@services-test-base';
import { HelpService, GlobalService } from '@service';
import ISetupResult from '@interfaces/service/ISetupResult';

describe('HelpService.setup()', () => {
	let { ctx } = baseVars();

	beforeAll(() => {
		const result = baseSetup();
		ctx = result.ctx;
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		const setup$ = HelpService.setup(ctx);
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: HelpService.name,
		};

		// Act
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toBe(true);
	});
});
