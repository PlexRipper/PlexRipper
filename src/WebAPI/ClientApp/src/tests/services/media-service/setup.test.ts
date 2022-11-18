import { describe, beforeAll, expect, test } from '@jest/globals';
import { subscribeSpyTo } from '@services-test-base';
import { MediaService } from '@service';
import { baseSetup, baseVars } from '~/tests/services/_base/base';
import ISetupResult from '@interfaces/service/ISetupResult';

describe('MediaService.setup()', () => {
	let { ctx } = baseVars();
	beforeAll(() => {
		const result = baseSetup();
		ctx = result.ctx;
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		const setup$ = MediaService.setup(ctx);
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: MediaService.name,
		};

		// Act
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toBe(true);
	});
});
