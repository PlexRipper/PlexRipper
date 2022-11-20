import Log from 'consola';
import { describe, beforeAll, expect, test } from '@jest/globals';
import { baseSetup } from "~/tests/services/_base/base";

describe('ProgressService.setup()', () => {
	let ctx;
	beforeAll(() => {
		const result = baseSetup();
		ctx = result.ctx;
	});

	test('Should return success and complete when setup is run', (done) => {
		// Arrange

		// Act

		// Assert
	});
});
