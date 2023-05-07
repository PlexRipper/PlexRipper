import { randEmail, randFullName, randPassword } from '@ngneat/falso';
import { route, apiRoute } from '@fixtures/baseE2E';
import { PLEX_ACCOUNT_RELATIVE_PATH } from '@api-urls';
import { generateResultDTO } from '@mock';

describe('Add Plex account to PlexRipper', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 2,
			plexServerCount: 5,
		});

		cy.visit(route('/settings/accounts')).as('setupPage');
	});

	it('Should create an Account when input is valid and close on save', () => {
		const account = {
			id: 100,
			isEnabled: true,
			isMain: true,
			displayName: randFullName(),
			username: randEmail(),
			password: randPassword(),
		};

		cy.getCy('account-overview-add-account').click();

		// Fill in credentials
		cy.getCy('account-form-display-name-input').type(account.displayName);
		cy.getCy('account-form-username-input').type(account.username);
		cy.getCy('account-form-password-input').type(account.password);

		// Validate Action
		cy.intercept('POST', apiRoute(PLEX_ACCOUNT_RELATIVE_PATH, '/validate'), {
			statusCode: 200,
			body: generateResultDTO(account),
		});
		cy.getCy('account-dialog-validate-button').click();

		// Create Action
		cy.intercept('POST', apiRoute(PLEX_ACCOUNT_RELATIVE_PATH), {
			statusCode: 200,
			body: generateResultDTO(account),
		});

		// Hide Account dialog
		cy.intercept('GET', apiRoute(PLEX_ACCOUNT_RELATIVE_PATH, '/' + account.id), {
			statusCode: 200,
			body: generateResultDTO(account),
		});
		cy.getCy('account-dialog-save-button').click();

		cy.getCy('account-dialog-form').should('not.exist');
	});
});
