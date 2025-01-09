import type { BasePageSetupResult } from '@fixtures';
import { generateResultDTO } from '@mock';
import { NotificationPaths } from '@api/api-paths';
import { headers } from '@fixtures';

export function setupMockNotificationsEndpoints(
	this: BasePageSetupResult,
): BasePageSetupResult {
	cy.intercept('GET', NotificationPaths.getAllNotificationsEndpoint(), {
		statusCode: 200,
		body: generateResultDTO([]), // Empty notification list
		...headers,
	});

	return this;
}
