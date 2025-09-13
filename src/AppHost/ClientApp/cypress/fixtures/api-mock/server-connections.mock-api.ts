import type { BasePageSetupResult } from '@fixtures';
import type { MockConfig } from '@mock';
import { generatePlexServerConnections, generateResultDTO } from '@mock';
import { PlexServerConnectionPaths } from '@api-urls';
import { headers } from '@fixtures';

export function setupMockPlexServerConnectionsEndpoints(this: BasePageSetupResult, config: MockConfig): BasePageSetupResult {
	for (const plexServer of this.plexServers) {
		this.plexServerConnections.push(...generatePlexServerConnections({ plexServerId: plexServer.id, config }));
	}

	if (config.override.plexServerConnections) {
		this.plexServerConnections = config.override.plexServerConnections(this.plexServerConnections);
	}

	cy.intercept('GET', PlexServerConnectionPaths.getAllPlexServerConnectionsEndpoint(), {
		statusCode: 200,
		body: generateResultDTO(this.plexServerConnections),
		...headers,
	}).then(() => {
		if (config.debugDisplayData) {
			cy.log('BasePageSetup -> plexServerConnections', this.plexServerConnections);
		}
	});

	return this;
}
