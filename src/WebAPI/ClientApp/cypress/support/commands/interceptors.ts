import { PlexAccountPaths } from '@api-urls';
import { generatePlexAccount, generateResultDTO } from '@mock';
import type { PlexAccountDTO, ValidatePlexAccountResponse } from '@dto';

/* eslint-disable @typescript-eslint/no-namespace */
declare global {
	namespace Cypress {
		// 🤔 unsure why this Subject is unused, nor what to do with it...
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		interface Chainable<Subject> {

			interceptValidatePlexAccount(config: {
				isUnAuthorized?: boolean;
				partialData?: Partial<PlexAccountDTO>;
			}): Chainable;
		}
	}
}

Cypress.Commands.add('interceptValidatePlexAccount', ({ isUnAuthorized = false, partialData = {} }: {
	isUnAuthorized?: boolean;
	partialData?: Partial<PlexAccountDTO>;
}) => {
	const plexAccount: PlexAccountDTO = generatePlexAccount({ id: 99 });

	return cy.intercept('POST', PlexAccountPaths.validatePlexAccountEndpoint(), {
		statusCode: 200,
		body: generateResultDTO({
			isUnAuthorized: isUnAuthorized,
			plexAccountDTO: {
				...plexAccount,
				...partialData,
			},
		} as ValidatePlexAccountResponse),
	});
});
