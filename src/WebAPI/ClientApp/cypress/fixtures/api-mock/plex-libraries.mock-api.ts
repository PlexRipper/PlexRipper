import type { BasePageSetupResult } from '@fixtures';
import type { MockConfig } from '@mock';
import { generatePlexLibrariesFromPlexServers, generateResultDTO } from '@mock';
import { PlexLibraryPaths } from '@api/api-paths';
import { headers } from '@fixtures';

export function setupMockPlexLibrariesEndpoints(
	this: BasePageSetupResult,
	config: MockConfig,
): BasePageSetupResult {
	this.plexLibraries = generatePlexLibrariesFromPlexServers({ plexServers: this.plexServers, config });

	if (config.override.plexLibraries) {
		this.plexLibraries = config.override.plexLibraries(this.plexLibraries);
	}

	cy.intercept('GET', PlexLibraryPaths.getAllPlexLibrariesEndpoint(), {
		statusCode: 200,
		body: generateResultDTO(this.plexLibraries),
		...headers,
	}).then(() => {
		if (config.debugDisplayData) {
			cy.log('BasePageSetup -> plexLibraries', this.plexLibraries);
		}
	});

	// Detail call for every library
	for (const library of this.plexLibraries) {
		cy.intercept('GET', PlexLibraryPaths.getPlexLibraryByIdEndpoint(library.id), {
			statusCode: 200,
			body: generateResultDTO(library),
			...headers,
		});
	}

	return this;
}
