import { randEmail, randFullName, randPassword } from '@ngneat/falso';
import { route, basePageSetup, apiRoute } from '@fixtures/baseE2E';
import { PLEX_ACCOUNT_RELATIVE_PATH } from '@api-urls';
import { generateResultDTO } from '@mock';

describe('Add Plex account to PlexRipper', () => {
	let data = {};

	beforeEach(() => {
		const config = {
			plexAccountCount: 2,
			plexServerCount: 5,
		};

		data = basePageSetup(config);

		cy.visit(route('/settings/accounts')).as('setupPage');
	});

	it('Should create an Account when input is valid', () => {
		const account = {
			id: 100,
			isEnabled: true,
			isMain: true,
			displayName: randFullName(),
			username: randEmail(),
			password: randPassword(),
		};

		cy.get('[data-cy="account-overview-add-account"]').click();

		// Fill in credentials
		cy.get('[data-cy="account-form-display-name-input"]').type(account.displayName);
		cy.get('[data-cy="account-form-username-input"]').type(account.username);
		cy.get('[data-cy="account-form-password-input"]').type(account.password);

		// Validate Action
		cy.intercept('POST', apiRoute(PLEX_ACCOUNT_RELATIVE_PATH, '/validate'), {
			statusCode: 200,
			body: generateResultDTO(account),
		});
		cy.get('[data-cy="account-dialog-validate-button"]').click();

		// Create Action
		cy.intercept('POST', apiRoute(PLEX_ACCOUNT_RELATIVE_PATH), {
			statusCode: 200,
			body: generateResultDTO(account),
		});
		cy.get('[data-cy="account-dialog-save-button"]').click();

		cy.intercept('GET', apiRoute(PLEX_ACCOUNT_RELATIVE_PATH, '/' + account.id), {
			statusCode: 200,
			body: generateResultDTO(account),
		});
	});
});
