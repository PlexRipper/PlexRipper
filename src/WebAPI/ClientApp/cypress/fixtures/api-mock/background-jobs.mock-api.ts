import type { BasePageSetupResult } from '@fixtures';
import type { MockConfig } from '@mock';
import { generateResultDTO } from '@mock';
import { headers } from '@fixtures';
import { BackgroundJobsPaths } from '@api/generated/BackgroundJobs';

export function setupMockBackgroundJobsEndpoints(this: BasePageSetupResult, config: MockConfig): BasePageSetupResult {
	cy.intercept('GET', BackgroundJobsPaths.getAllBackgroundJobsEndpoint(), {
		statusCode: 200,
		body: generateResultDTO([]),
		...headers,
	}).then(() => {
		if (config.debugDisplayData) {
			cy.log('BasePageSetup -> backgroundJobsPaths', this.plexServerConnections);
		}
	});

	return this;
}
