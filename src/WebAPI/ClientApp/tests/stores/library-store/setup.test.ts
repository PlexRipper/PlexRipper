import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { subscribeSpyTo, baseSetup, getAxiosMock, baseVars } from '@services-test-base';
import { generateResultDTO } from '@mock';
import { PlexLibraryPaths } from '@api/api-paths';
import type { ISetupResult } from '@interfaces';
import { useLibraryStore } from '#build/imports';

describe('LibraryStore.setup()', () => {
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
		const libraryStore = useLibraryStore();

		mock.onGet(PlexLibraryPaths.getAllPlexLibrariesEndpoint()).reply(200, generateResultDTO([]));
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: useLibraryStore.name,
		};

		// Act
		const result = subscribeSpyTo(libraryStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});
});
