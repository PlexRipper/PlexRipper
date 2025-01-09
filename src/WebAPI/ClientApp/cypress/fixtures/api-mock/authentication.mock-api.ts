import type { MockConfig } from '@mock';
import type { BasePageSetupResult } from '@fixtures';
import { AuthenticationPaths } from '@api-urls';
import { generateResultDTO } from '@mock';
import type { AppCredentialsDTO } from '@dto';
import { randPassword } from '@ngneat/falso';

export function setupMockAuthenticationEndpoints(this: BasePageSetupResult, config: MockConfig): BasePageSetupResult {
	cy.interceptAuthenticationStatus(config.isLoggedIn, config.pageLoadDelay).then(() => {
		if (config.debugDisplayData) {
			cy.log('BasePageSetup -> authentication status', config.isLoggedIn);
		}
	});

	cy.intercept('GET', AuthenticationPaths.getAppCredentials(), {
		statusCode: 200,
		body: generateResultDTO<AppCredentialsDTO>({
			isDefaultCredentials: true,
			userName: 'PlexRipperRocks',
			password: 'Pl€XR!ℙℙ€R69',
		}),
		// Headers are not sent with authentication requests
		// ...headers,
	});

	cy.intercept('PUT', AuthenticationPaths.updateCredentialsEndpoint(), (req) => {
		req.reply(
			{
				statusCode: 200,
				body: generateResultDTO<AppCredentialsDTO>({
					isDefaultCredentials: false,
					userName: req.body.userName, // Use the username from the request
					password: randPassword({ size: 16 }) + '$%&',
				}),
			});
		// Headers are not sent with authentication requests
		// ...headers,
	});

	return this;
}
