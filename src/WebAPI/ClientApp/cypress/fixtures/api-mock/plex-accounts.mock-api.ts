import type { BasePageSetupResult } from '@fixtures';
import type { MockConfig } from '@mock';
import { generatePlexAccounts, generateResultDTO } from '@mock';
import { PlexAccountPaths } from '@api/api-paths';
import { headers } from '@fixtures';

export function setupMockPlexAccountsEndpoints(
	this: BasePageSetupResult,
	config: MockConfig,
): BasePageSetupResult {
	this.plexAccounts = generatePlexAccounts({
		config,
		plexServers: this.plexServers,
		plexLibraries: this.plexLibraries,
	});

	if (config.override.plexAccounts) {
		this.plexAccounts = config.override.plexAccounts(this.plexAccounts);
	}

	cy.intercept('GET', PlexAccountPaths.getAllPlexAccountsEndpoint(), {
		statusCode: 200,
		body: generateResultDTO(this.plexAccounts),
		...headers,
	}).then(() => {
		if (config.debugDisplayData) {
			cy.log('BasePageSetup -> plexAccounts', this.plexAccounts);
		}
	});

	return this;
}
