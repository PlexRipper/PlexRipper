import type { BasePageSetupResult } from '@fixtures';
import { headers } from '@fixtures';

export function setupMockSignalREndpoints(this: BasePageSetupResult): BasePageSetupResult {
	cy.intercept('GET', '/progress', {
		statusCode: 200,
		body: {},
		...headers,
	});

	cy.intercept('GET', '/notifications', {
		statusCode: 200,
		body: {},
		...headers,
	});

	return this;
}
