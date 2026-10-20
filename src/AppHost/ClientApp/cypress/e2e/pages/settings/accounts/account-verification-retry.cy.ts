import { route } from '@fixtures';
import { PlexAccountPaths } from '@api-urls';
import { generateFailedResultDTO, generatePlexAccount } from '@mock';

describe('Two-factor verification retry feedback', () => {
	it('Should show inline failure after a challenge and preserve credentials for a successful retry', () => {
		const account = generatePlexAccount({ id: 99, partialData: { is2Fa: true, isValidated: false } });
		cy.basePageSetup({ plexAccountCount: 0, plexServerCount: 0 });
		cy.visit(route('/settings/accounts'));
		cy.getCy('account-overview-add-account').click();
		cy.getCy('account-form-display-name-input').type(account.displayName);
		cy.getCy('account-form-username-input').type(account.username);
		cy.getCy('account-form-password-input').type(account.password);
		cy.validatePlexCredentialsEndpoint({
			isUnAuthorized: true,
			partialData: { clientId: account.clientId, is2Fa: true, isValidated: false },
		}).as('challenge');
		cy.getCy('account-dialog-validate-button').click();
		cy.wait('@challenge');
		cy.getCy('2fa-code-verification-dialog').should('be.visible');
		cy.getCy('2fa-code-verification-error').should('not.exist');

		cy.intercept('POST', PlexAccountPaths.validatePlexCredentialsEndpoint(), {
			statusCode: 500, body: generateFailedResultDTO({ statusCode: 500 }),
		}).as('failedVerification');
		cy.getCy('2fa-code-verification-dialog').find('[data-test="single-input"]').first().type('111111');
		cy.wait('@failedVerification').its('request.body').should('include', {
			username: account.username, password: account.password, clientId: account.clientId, verificationCode: '111111',
		});
		cy.getCy('api-error').should('contain.text', 'HTTP 500');
		cy.getCy('close-alert-dialog').click();
		cy.getCy('2fa-code-verification-dialog').should('be.visible');
		cy.getCy('2fa-code-verification-error').should('be.visible');
		cy.getCy('2fa-code-verification-dialog').find('[data-test="single-input"]').each(($input) => {
			cy.wrap($input).should('have.value', '');
		});
		cy.getCy('2fa-code-verification-confirm-button').should('be.disabled');
		cy.getCy('account-dialog-save-button').should('be.disabled');

		cy.validatePlexCredentialsEndpoint({
			partialData: {
				clientId: account.clientId, username: account.username, password: account.password,
				is2Fa: true, isValidated: true,
			},
		}).as('successfulVerification');
		cy.getCy('2fa-code-verification-dialog').find('[data-test="single-input"]').first().type('123456');
		cy.wait('@successfulVerification').its('request.body').should('include', {
			username: account.username, password: account.password, clientId: account.clientId, verificationCode: '123456',
		});
		cy.getCy('2fa-code-verification-dialog').should('not.exist');
		cy.getCy('account-form-username-input').should('have.value', account.username);
		cy.getCy('account-form-password-input').should('have.value', account.password);
		cy.getCy('account-dialog-save-button').should('not.be.disabled');
	});
});
