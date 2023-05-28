import { basePageSetup } from '@fixtures/baseE2E';
import { cy, describe, it } from 'local-cypress';
import { PLEX_ACCOUNT_API_URL } from '@api-urls';
import { generateResultDTO } from '@mock';
import { generatePlexAccountServerAndLibraries } from '@mock/mock-combination';
import { PlexAccountDTO } from '@dto/mainApi';

describe('PlexAccount creation through the account dialog', () => {
	it('Should create a PlexAccount when credentials are valid', () => {
		const config = {
			seed: 267398,
			plexAccountCount: 2,
			plexServerCount: 5,
		};

		basePageSetup(config);

		const { account } = generatePlexAccountServerAndLibraries(config);

		cy.visit('/settings/accounts');
		// Add new account
		cy.get('[data-cy="account-overview-add-account"]').click();
		cy.get('[data-cy="account-dialog-form"]').parent().should('be.visible');

		// Fill in credentials
		cy.get('[data-cy="account-form-display-name-input"]').type(account.displayName);
		cy.get('[data-cy="account-form-username-input"]').type(account.username);
		cy.get('[data-cy="account-form-password-input"]').type(account.password);

		// Validate Action
		cy.intercept('POST', PLEX_ACCOUNT_API_URL + '/validate', {
			statusCode: 200,
			body: generateResultDTO(account),
		}).as('validate-account');
		cy.get('[data-cy="account-dialog-save-button"]').should('be.disabled');
		cy.get('[data-cy="account-dialog-validate-button"]').click();

		// Save Action
		cy.intercept('POST', PLEX_ACCOUNT_API_URL, {
			statusCode: 200,
			body: generateResultDTO(account),
		}).as('create-account');
		cy.get('[data-cy="account-dialog-save-button"]').click();

		// The just created account is retrieved on save
		cy.intercept('GET', PLEX_ACCOUNT_API_URL + '/1', {
			statusCode: 200,
			body: generateResultDTO(account),
		});

		// Assert
		cy.wait('@validate-account').then((intercept) => {
			const reqAccount = intercept.request.body as PlexAccountDTO;
			// console.log('validate-account', reqAccount);
			cy.wrap(reqAccount.displayName).should('eq', account.displayName);
			cy.wrap(reqAccount.username).should('eq', account.username);
			cy.wrap(reqAccount.password).should('eq', account.password);
		});

		cy.wait('@create-account').then((intercept) => {
			const reqAccount = intercept.request.body as PlexAccountDTO;
			// console.log('create-account', reqAccount);
			cy.wrap(reqAccount.displayName).should('eq', account.displayName);
			cy.wrap(reqAccount.username).should('eq', account.username);
			cy.wrap(reqAccount.password).should('eq', account.password);
			cy.wrap(reqAccount.isValidated).should('eq', true);
		});
		cy.get('[data-cy="account-dialog-form"]').parent().should('not.be.visible');
	});
});
