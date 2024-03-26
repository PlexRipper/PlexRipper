import { describe, beforeAll, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { subscribeSpyTo, baseSetup, getAxiosMock, baseVars } from '@services-test-base';
import { generateResultDTO } from '@mock';
import { DOWNLOAD_RELATIVE_PATH } from '@api-urls';
import type { ISetupResult } from '@interfaces';
import { useDownloadStore } from '#build/imports';

describe('DownloadStore.setup()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		const downloadStore = useDownloadStore();
		mock.onGet(DOWNLOAD_RELATIVE_PATH).reply(200, generateResultDTO([]));
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: useDownloadStore.name,
		};

		// Act
		const result = subscribeSpyTo(downloadStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
