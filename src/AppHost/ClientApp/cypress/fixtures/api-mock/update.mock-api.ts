import type { BasePageSetupResult } from '@fixtures';
import { generateResultDTO } from '@mock';

import { UpdatePaths } from '@api/api-paths';

export function setupMockUpdateEndpoints(
	this: BasePageSetupResult,
): BasePageSetupResult {
	cy.intercept('GET', `**${UpdatePaths.checkForUpdateEndpoint()}*`, {
		statusCode: 200,
		body: generateResultDTO({
			currentVersion: '9.9.9',
			isUpdateAvailable: false,
			newestVersion: '9.9.9',
			releaseNotes: [],
		}),
	});

	return this;
}
