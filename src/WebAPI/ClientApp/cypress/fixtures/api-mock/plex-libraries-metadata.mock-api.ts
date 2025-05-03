import type { BasePageSetupResult } from '@fixtures';
import { type MockConfig, Seed } from '@mock';
import { generatePlexLibrariesFromPlexServers, generateResultDTO } from '@mock';
import { PlexLibraryPaths } from '@api/api-paths';
import { headers } from '@fixtures';

export function setupMockPlexLibraryMetaDataEndpoints(
	this: BasePageSetupResult,
	config: MockConfig,
): BasePageSetupResult {
	this.plexLibraries = generatePlexLibrariesFromPlexServers({ seed: new Seed(config.seed),
		plexServers: this.plexServers, config });

	if (config.override.plexLibraries) {
		this.plexLibraries = config.override.plexLibraries(this.plexLibraries);
	}

	// Detail call for every library
	for (const library of this.plexLibraries) {
		cy.intercept('GET', PlexLibraryPaths.getLibraryMediaMetadata(library.id, {
			mediaType: library.type,
		}), {
			statusCode: 200,
			body: generateResultDTO([]),
			...headers,
		}).then(() => {
			if (config.debugDisplayData) {
				cy.log('BasePageSetup -> plexLibraries', this.plexLibraries);
			}
		});
	}

	return this;
}
