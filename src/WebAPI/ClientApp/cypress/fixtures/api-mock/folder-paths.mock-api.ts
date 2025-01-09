import type { BasePageSetupResult } from '@fixtures';
import { headers, MemoryFileSystem } from '@fixtures';
import type { MockConfig } from '@mock';
import { generateDefaultFolderPaths, generateFileSystemModelDTO, generateResultDTO } from '@mock';
import { FolderPathPaths } from '@api/api-paths';
import { type FileSystemDTO, FileSystemEntityType, type FolderPathDTO } from '@dto';

export function setupMockFolderPathsEndpoints(
	this: BasePageSetupResult,
	config: MockConfig,
): BasePageSetupResult {
	this.folderPaths = generateDefaultFolderPaths({ config });
	if (config.override.folderPaths) {
		this.folderPaths = config.override.folderPaths(this.folderPaths);
	}

	const vol = createFileSystem();

	cy.intercept('GET', FolderPathPaths.getAllFolderPathsEndpoint(), {
		statusCode: 200,
		body: generateResultDTO(this.folderPaths),
		...headers,
	});

	cy.intercept('GET', FolderPathPaths.getFolderPathDirectoryEndpoint({
		path: '', // This is a wildcard to match any path
	}) + '*', (req) => {
		const path: string = req.query.path as string || '/';
		const exists = vol.directoryExists(path);
		const fsResult: FileSystemDTO = {
			current: generateFileSystemModelDTO({
				path, partialData: {
					hasReadPermission: exists,
					hasWritePermission: exists,
					type: FileSystemEntityType.Folder,
				},
			}),
			// Get directory contents and filter out non-
			directories: vol.listDirectory(path).map((x) => generateFileSystemModelDTO({ path: '/' + x })),
			files: [],
			parent: '',
		};
		req.reply({
			statusCode: 200,
			body: generateResultDTO(fsResult),
			...headers,
		});
	});

	// Update folder path
	cy.intercept('PUT', FolderPathPaths.updateFolderPathEndpoint(), (req) => {
		const updatedFolderPath = req.body as FolderPathDTO;

		const folderPath = this.folderPaths.find((x) => x.id === updatedFolderPath.id);
		if (folderPath) {
			Object.assign(folderPath, updatedFolderPath);
			folderPath.isValid = vol.directoryExists(folderPath.directory);
		}
		req.reply({
			statusCode: 200,
			body: generateResultDTO(folderPath),
			...headers,
		});
	});

	return this;
}

function createFileSystem() {
	const defaultFolderPaths = generateDefaultFolderPaths();
	const fs = new MemoryFileSystem();

	for (const defaultFolderPath of defaultFolderPaths) {
		fs.createDirectory(defaultFolderPath.directory + defaultFolderPath.directory + 'Inside');
	}

	return fs;
}
