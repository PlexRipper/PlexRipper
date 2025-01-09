import { PlexAccountPaths } from '@api-urls';
import { generatePlexAccount, generateResultDTO } from '@mock';
import type { PlexAccountDTO } from '@dto';
import { headers } from '@fixtures';

Cypress.Commands.add('createPlexAccount', (account: PlexAccountDTO | null) => {
	cy.getCy('account-overview-add-account').click();

	if (account === null) {
		account = generatePlexAccount({
			id: 99,
			partialData: {
				isValidated: false,
			},
		});
	}

	// Fill in credentials
	cy.getCy('account-form-display-name-input').type(account.displayName);
	cy.getCy('account-form-username-input').type(account.username);
	cy.getCy('account-form-password-input').type(account.password);

	// Validate Action
	cy.intercept('POST', PlexAccountPaths.validatePlexAccountEndpoint(), {
		statusCode: 200,
		body: generateResultDTO({ ...account, isValidated: true, is2Fa: false }),
	});
	cy.getCy('account-dialog-validate-button').click();

	// Create Action
	cy.intercept('POST', PlexAccountPaths.createPlexAccountEndpoint(), {
		statusCode: 200,
		body: generateResultDTO(account),
	});

	// Hide Account dialog
	cy.intercept('GET', PlexAccountPaths.getPlexAccountByIdEndpoint(account.id), {
		statusCode: 200,
		body: generateResultDTO(account),
	});

	cy.intercept('GET', PlexAccountPaths.getAllPlexAccountsEndpoint(), {
		statusCode: 200,
		body: generateResultDTO([{ ...account, isValidated: true }]),
		...headers,
	});

	cy.getCy('account-dialog-save-button').click();
	cy.getCy('account-dialog-form').should('not.exist');
	cy.getCy('account-card-id-99').should('exist');
});

export {};
