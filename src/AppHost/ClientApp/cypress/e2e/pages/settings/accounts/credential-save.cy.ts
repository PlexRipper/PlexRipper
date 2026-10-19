import { route } from '@fixtures';
import { AuthenticationPaths } from '@api-urls';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import type { AppCredentialsDTO } from '@dto';

describe('App credential save recovery', () => {
	it('Should keep a rejected username editable and report success only after a corrected save', () => {
		cy.basePageSetup({ isLoggedIn: true, plexAccountCount: 0, plexServerCount: 0 }).then(() => {
			let credentials: AppCredentialsDTO = {
				userName: 'ExistingUser22', password: 'masked-password', isDefaultCredentials: false,
			};
			let saveAttempts = 0;
			cy.intercept('GET', '**/api/Integration', { statusCode: 200, body: generateResultDTO([]) });
			cy.intercept('GET', AuthenticationPaths.getAppCredentials(), (request) => {
				request.reply({ statusCode: 200, body: generateResultDTO(credentials) });
			}).as('getCredentials');
			cy.intercept('PUT', AuthenticationPaths.updateCredentialsEndpoint(), (request) => {
				saveAttempts++;
				expect(request.body.password).to.equal(null);
				if (saveAttempts === 1) {
					expect(request.body.username).to.equal('Invalid Username');
					request.reply({
						statusCode: 400,
						body: generateFailedResultDTO({
							statusCode: 400,
							errors: [{ message: 'Username contains an invalid character', reasons: [], metadata: {} }],
						}),
					});
					return;
				}
				expect(request.body.username).to.equal('RecoveredUser22');
				credentials = { ...credentials, userName: 'RecoveredUser22' };
				request.reply({ statusCode: 200, body: generateResultDTO(null) });
			}).as('saveCredentials');

			cy.visit(route('/settings/accounts'));
			cy.wait('@getCredentials');
			cy.getCy('app-username-input').should('have.value', 'ExistingUser22').clear().type('Invalid Username');
			cy.then(() => expect(saveAttempts).to.equal(0));
			cy.getCy('save-credentials-button').should('not.be.disabled').click();
			cy.wait('@saveCredentials').its('response.statusCode').should('equal', 400);
			cy.getCy('alert-dialog').should('be.visible');
			cy.getCy('api-error').should('contain.text', 'Username contains an invalid character');
			cy.getCy('save-credentials-button').should('have.class', 'base-btn-negative');
			cy.getCy('close-alert-dialog').click();
			cy.getCy('app-username-input').should('have.value', 'Invalid Username');
			cy.get('@getCredentials.all').should('have.length', 1);

			cy.getCy('app-username-input').clear().type('RecoveredUser22');
			cy.getCy('save-credentials-button').should('not.be.disabled').click();
			cy.wait('@saveCredentials').its('response.statusCode').should('equal', 200);
			cy.wait('@getCredentials').its('response.body.value.userName').should('equal', 'RecoveredUser22');
			cy.getCy('save-credentials-button').should('have.class', 'base-btn-positive').and('be.disabled');
			cy.getCy('app-username-input').should('have.value', 'RecoveredUser22');
			cy.then(() => expect(saveAttempts).to.equal(2));
			cy.reload();
			cy.wait('@getCredentials');
			cy.getCy('app-username-input').should('have.value', 'RecoveredUser22');
		});
	});
});
