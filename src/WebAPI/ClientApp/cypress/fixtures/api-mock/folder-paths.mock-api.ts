import type { BasePageSetupResult } from '@fixtures';
import type { MockConfig } from '@mock';
import { generateDefaultFolderPaths, generateResultDTO } from '@mock';
import { FolderPathPaths } from '@api/api-paths';
import { headers } from '@fixtures';

export function setupMockFolderPathsEndpoints(
	this: BasePageSetupResult,
	config: MockConfig,
): BasePageSetupResult {
	this.folderPaths = generateDefaultFolderPaths({ config });

	cy.intercept('GET', FolderPathPaths.getAllFolderPathsEndpoint(), {
		statusCode: 200,
		body: generateResultDTO(this.folderPaths),
		...headers,
	});

	return this;
}
