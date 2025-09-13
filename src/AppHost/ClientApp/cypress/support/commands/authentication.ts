import { AuthenticationPaths } from '@api-urls';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import type { UserClaimsDTO } from '@dto';

Cypress.Commands.add('interceptAuthenticationStatus', (loggedIn: boolean, pageLoadDelay: number = 0) => {
	cy.intercept('GET', AuthenticationPaths.authenticationStatusEndpoint(), {
		delay: pageLoadDelay,
		statusCode: loggedIn ? 200 : 401,
		body: loggedIn
			? generateResultDTO<UserClaimsDTO>({
					userName: 'admin',
					claims: ['Admin'],
					isLoggedIn: loggedIn,
				})
			: generateFailedResultDTO(),
	});
});
