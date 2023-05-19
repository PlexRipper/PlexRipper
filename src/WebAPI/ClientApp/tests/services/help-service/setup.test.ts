import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { subscribeSpyTo, baseSetup, baseVars } from '@services-test-base';
import HelpService from '@service/helpService';
import ISetupResult from '@interfaces/service/ISetupResult';

describe('HelpService.setup()', () => {
	baseVars();

	beforeAll(() => {
		baseSetup();
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		const setup$ = HelpService.setup();
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: HelpService.name,
		};

		// Act
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
