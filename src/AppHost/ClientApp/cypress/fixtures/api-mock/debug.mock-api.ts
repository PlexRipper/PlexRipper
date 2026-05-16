import type { BasePageSetupResult } from '@fixtures';
import { generateResultDTO } from '@mock';
import { DebugPaths } from '@api/api-paths';

export function setupMockDebugEndpoints(
	this: BasePageSetupResult,
): BasePageSetupResult {
	cy.intercept('GET', `**${DebugPaths.getAllLogsEndpoint()}*`, {
		statusCode: 200,
		body: generateResultDTO([]),
	});

	return this;
}
