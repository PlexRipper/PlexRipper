import { route, apiRoute } from '@fixtures/baseE2E';
import { PLEX_ACCOUNT_RELATIVE_PATH } from '@api-urls';
import { generatePlexAccount, generateResultDTO } from '@mock';
import { PlexAccountDTO } from '@dto/mainApi';

describe('Add Plex account to PlexRipper', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 2,
			plexServerCount: 5,
		});

		cy.visit(route('/settings/accounts')).as('setupPage');
	});

	it('Should create an Account when input is valid and close on save', () => {
		const account: PlexAccountDTO = generatePlexAccount({
			id: 99,
			partialData: {
				isValidated: false,
			},
		});

		cy.getCy('account-overview-add-account').click();

		// Fill in credentials
		cy.getCy('account-form-display-name-input').type(account.displayName);
		cy.getCy('account-form-username-input').type(account.username);
		cy.getCy('account-form-password-input').type(account.password);

		// Validate Action
		cy.intercept('POST', apiRoute(PLEX_ACCOUNT_RELATIVE_PATH, '/validate'), {
			statusCode: 200,
			body: generateResultDTO({ ...account, isValidated: true, is2Fa: false }),
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

	it('Should request a verification code when 2Fa is enabled for an Plex account', function () {
		const account: PlexAccountDTO = generatePlexAccount({ id: 99 });

		cy.getCy('account-overview-add-account').click();

		// Fill in credentials
		cy.getCy('account-form-display-name-input').type(account.displayName);
		cy.getCy('account-form-username-input').type(account.username);
		cy.getCy('account-form-password-input').type(account.password);

		// Validate Action
		cy.intercept('POST', apiRoute(PLEX_ACCOUNT_RELATIVE_PATH, '/validate'), {
			statusCode: 200,
			body: generateResultDTO({
				...account,
				isValidated: true,
				is2Fa: true,
			}),
		});
		cy.getCy('account-dialog-validate-button').click();
		cy.getCy('2fa-code-verification-dialog').should('exist');
		// Insert verification code
		cy.get(':nth-child(1) > [data-test="single-input"]').type('123456');

		// Create Action
		cy.intercept('POST', apiRoute(PLEX_ACCOUNT_RELATIVE_PATH), {
			statusCode: 200,
			body: generateResultDTO(account),
		});

		cy.intercept('GET', apiRoute(PLEX_ACCOUNT_RELATIVE_PATH, '/' + account.id), {
			statusCode: 200,
			body: generateResultDTO(account),
		});

		cy.getCy('account-dialog-save-button').click();
		cy.getCy('account-dialog-form').should('not.exist');
	});
});
