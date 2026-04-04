import { beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { firstValueFrom, throwError } from 'rxjs';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { generateFailedResultDTO } from '@mock';
import { folderPathApi } from '@api';
import { FolderPathPaths } from '@api-urls';
import { FolderType, PlexMediaType, type FolderPathDTO } from '@dto';
import { StoreNames, type ISetupResult } from '@interfaces';
import { useFolderPathStore } from '@store';

describe('FolderPathStore regressions', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
		vi.restoreAllMocks();
	});

	function createFolderPath(partial: Partial<FolderPathDTO> = {}): FolderPathDTO {
		return {
			id: partial.id ?? 7,
			displayName: partial.displayName ?? 'Movies',
			directory: partial.directory ?? '/movies',
			folderType: partial.folderType ?? FolderType.MovieFolder,
			mediaType: partial.mediaType ?? PlexMediaType.Movie,
			isValid: partial.isValid ?? true,
			isDefault: partial.isDefault ?? false,
		};
	}

	test('Should report setup failure when refreshing folder paths fails', async () => {
		// Arrange
		const folderPathStore = useFolderPathStore();
		const failedSetupResult: ISetupResult = {
			isSuccess: false,
			name: StoreNames.FolderPathStore,
		};
		mock.onGet(FolderPathPaths.getAllFolderPathsEndpoint()).reply(500, generateFailedResultDTO({ statusCode: 500 }));

		// Act
		const result = subscribeSpyTo(folderPathStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(failedSetupResult);
	});

	test('Should keep the original directory when updating a folder path directory fails', async () => {
		// Arrange
		const folderPath = createFolderPath();
		const folderPathStore = useFolderPathStore();
		folderPathStore.folderPaths = [folderPath];
		mock.onPut(FolderPathPaths.updateFolderPathEndpoint()).reply(500, generateFailedResultDTO({ statusCode: 500 }));

		// Act
		const result = subscribeSpyTo(folderPathStore.setFolderPathDirectory(folderPath.id, '/failed-directory'));
		await result.onComplete();

		// Assert
		expect(folderPathStore.getFolderPath(folderPath.id)?.directory).toBe('/movies');
	});

	test('Should keep the original display name when updating a folder path name fails', async () => {
		// Arrange
		const folderPath = createFolderPath();
		const folderPathStore = useFolderPathStore();
		folderPathStore.folderPaths = [folderPath];
		mock.onPut(FolderPathPaths.updateFolderPathEndpoint()).reply(500, generateFailedResultDTO({ statusCode: 500 }));

		// Act
		const result = subscribeSpyTo(folderPathStore.setFolderPathDisplayName(folderPath.id, 'Broken Name'));
		await result.onComplete();

		// Assert
		expect(folderPathStore.getFolderPath(folderPath.id)?.displayName).toBe('Movies');
	});

	test('Should keep the folder path when deleting it fails', async () => {
		// Arrange
		const folderPath = createFolderPath();
		const folderPathStore = useFolderPathStore();
		folderPathStore.folderPaths = [folderPath];
		mock.onDelete(FolderPathPaths.deleteFolderPathEndpoint(folderPath.id)).reply(500, generateFailedResultDTO({ statusCode: 500 }));

		// Act
		const result = subscribeSpyTo(folderPathStore.deleteFolderPath(folderPath.id));
		await result.onComplete();

		// Assert
		expect(folderPathStore.getFolderPaths()).toHaveLength(1);
		expect(folderPathStore.getFolderPath(folderPath.id)?.id).toBe(folderPath.id);
	});

	test('Should keep the folder path when deleting it errors', async () => {
		// Arrange
		const folderPath = createFolderPath();
		const folderPathStore = useFolderPathStore();
		folderPathStore.folderPaths = [folderPath];
		vi.spyOn(folderPathApi, 'deleteFolderPathEndpoint').mockReturnValue(
			throwError(() => new Error('Delete failed')),
		);

		// Act
		await expect(firstValueFrom(folderPathStore.deleteFolderPath(folderPath.id))).rejects.toThrow('Delete failed');

		// Assert
		expect(folderPathStore.getFolderPaths()).toHaveLength(1);
		expect(folderPathStore.getFolderPath(folderPath.id)?.id).toBe(folderPath.id);
	});
});
