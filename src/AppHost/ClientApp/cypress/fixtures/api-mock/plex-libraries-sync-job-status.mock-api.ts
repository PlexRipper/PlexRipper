import type { BasePageSetupResult } from '@fixtures';
import { headers } from '@fixtures';
import { generateResultDTO, type MockConfig } from '@mock';
import { PlexLibraryPaths } from '@api/api-paths';
import { type LibrarySyncJobQueueDTO, LibrarySyncJobStatus } from '@dto';

export function setupMockPlexLibrarySyncJobStatusEndpoints(this: BasePageSetupResult, config: MockConfig): BasePageSetupResult {
	this.plexLibrarySyncJobStatuses = this.plexLibraries.map<LibrarySyncJobQueueDTO>((x) => ({
		plexLibraryId: x.id,
		status: LibrarySyncJobStatus.Queued,
		completedAt: null,
		createdAt: new Date().toISOString(),
		errorMessage: null,
		plexServerId: x.plexServerId,
		priority: 1,
		startedAt: null,
		isServerOffline: false,
	}));

	// Detail call for every library
	cy.intercept('GET', PlexLibraryPaths.getLibrarySyncStatusEndpoint(), {
		statusCode: 200, body: generateResultDTO(this.plexLibrarySyncJobStatuses), ...headers,
	}).then(() => {
		if (config.debugDisplayData) {
			cy.log('BasePageSetup -> plexLibraries', this.plexLibraries);
		}
	});

	return this;
}
