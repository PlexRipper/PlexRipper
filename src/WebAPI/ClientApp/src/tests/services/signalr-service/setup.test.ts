import { describe, beforeAll, expect, test } from '@jest/globals';
import { subscribeSpyTo, baseSetup, baseVars } from '@services-test-base';
import { SignalrService } from '@service';
import ISetupResult from '@interfaces/service/ISetupResult';

describe('SignalrService.setup()', () => {
	let { ctx } = baseVars();

	beforeAll(() => {
		const result = baseSetup();
		ctx = result.ctx;
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		const setup$ = SignalrService.setup(ctx);
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: SignalrService.name,
		};

		// Act
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toBe(true);
	});
});
