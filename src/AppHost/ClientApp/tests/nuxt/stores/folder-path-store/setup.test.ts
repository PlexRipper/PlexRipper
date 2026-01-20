import { describe, beforeAll, test, expect, beforeEach } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { subscribeSpyTo, baseSetup, getAxiosMock, baseVars } from '@services-test-base';
import { generateResultDTO } from '@mock';
import { FolderPathPaths } from '@api/api-paths';
import { StoreNames, type ISetupResult } from '@interfaces';
import { useFolderPathStore } from '@store';

describe('FolderPathStore.setup()', () => {
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
		const folderPathStore = useFolderPathStore();
		mock.onGet(FolderPathPaths.getAllFolderPathsEndpoint()).reply(200, generateResultDTO([]));
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: StoreNames.FolderPathStore,
		};

		// Act
		const result = subscribeSpyTo(folderPathStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
