import { describe, beforeAll, test, expect } from 'vitest';
import { subscribeSpyTo, baseSetup } from '@services-test-base';
import MediaService from '@service/mediaService';
import ISetupResult from '@interfaces/service/ISetupResult';

describe('MediaService.setup()', () => {
	beforeAll(() => {
		baseSetup();
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		const setup$ = MediaService.setup();
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: MediaService.name,
		};

		// Act
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
