import { describe, beforeAll, test, expect } from 'vitest';
import { subscribeSpyTo, baseSetup, getAxiosMock, baseVars } from '@services-test-base';
import DownloadService from '@service/downloadService';
import { generateResultDTO } from '@mock';
import { DOWNLOAD_RELATIVE_PATH } from '@api-urls';
import ISetupResult from '@interfaces/service/ISetupResult';

describe('DownloadService.setup()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		mock.onGet(DOWNLOAD_RELATIVE_PATH).reply(200, generateResultDTO([]));
		const setup$ = DownloadService.setup();
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: DownloadService.name,
		};

		// Act
		const result = subscribeSpyTo(setup$);
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
