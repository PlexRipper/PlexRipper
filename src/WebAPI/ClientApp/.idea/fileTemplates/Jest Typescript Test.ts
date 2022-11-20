import Log from 'consola';
import { describe, beforeAll, expect, test } from '@jest/globals';
import { baseSetup } from '@services-test-base';;

describe('ProgressService.setup()', () => {
	let { ctx, mock, config } = baseVars();

	beforeAll(() => {
		const result = baseSetup();
		ctx = result.ctx;
	});
	
	beforeEach(() => {
		mock = getAxiosMock();
	});
	
	test('Should return success and complete when setup is run', (done) => {
		// Arrange

		// Act

		// Assert
	});
});
