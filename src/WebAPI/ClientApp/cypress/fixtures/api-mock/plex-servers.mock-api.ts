import { type BasePageSetupResult, headers } from '@fixtures';
import type { MockConfig } from '@mock';
import { generatePlexServers, generateResultDTO } from '@mock';
import { PlexServerPaths } from '@api-urls';

export function setupMockPlexServersEndpoints(this: BasePageSetupResult, config: MockConfig): BasePageSetupResult {
	this.plexServers = generatePlexServers({ config });
	if (config.override.plexServer) {
		this.plexServers = config.override.plexServer(generatePlexServers({ config }));
	}

	cy.intercept('GET', PlexServerPaths.getAllPlexServersEndpoint(), {
		statusCode: 200,
		body: generateResultDTO(this.plexServers),
		...headers,
	}).then(() => {
		if (config.debugDisplayData) {
			cy.log('BasePageSetup -> plexServers', this.plexServers);
		}
	});

	return this;
}
