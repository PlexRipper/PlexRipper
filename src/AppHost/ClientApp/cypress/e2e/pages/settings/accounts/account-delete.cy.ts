import { route } from '@fixtures';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import { PlexAccountPaths } from '@api-urls';

describe('Remove Plex account from Reaparr', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 2,
			plexServerCount: 5,
		});

		cy.visit(route('/settings/accounts'));
	});

	it('Should delete a Plex account when the delete button is clicked and confirmed', () => {
		cy.getPageData().then(({ plexAccounts }) => {
			const plexAccount = plexAccounts[1];
			if (!plexAccount) {
				throw new Error('Expected at least two accounts in test setup');
			}

			cy.getCy(`account-card-id-${plexAccount.id}`).click();

			cy.getCy('account-dialog-delete-button').click();

			cy.getCy('confirmation-dialog').should('be.visible');

			// Delete Action
			cy.intercept('DELETE', PlexAccountPaths.deletePlexAccountByIdEndpoint(plexAccount.id), {
				statusCode: 200,
				body: generateResultDTO(true),
			}).as('deleteAccount');

			// Return the accounts without the deleted one
			cy.intercept('GET', PlexAccountPaths.getAllPlexAccountsEndpoint(), {
				statusCode: 200,
				body: generateResultDTO(plexAccounts.filter((x) => x.id !== plexAccount.id)),
			}).as('getAccountsAfterDelete');

			cy.getCy('confirmation-dialog-confirmation-button').click();
			cy.wait('@deleteAccount');
			cy.wait('@getAccountsAfterDelete');

			cy.getCy('account-dialog-form').should('not.exist');

			cy.getCy(`account-card-id-${plexAccount.id}`).should('not.exist');
		});
	});

	it('Should close the account dialog and retain other cards when deletion succeeds but refresh fails', () => {
		cy.getPageData().then(({ plexAccounts }) => {
			const target = plexAccounts[1];
			const retained = plexAccounts[0];
			if (!target || !retained) throw new Error('Expected two accounts in test setup');
			cy.getCy(`account-card-id-${target.id}`).click();
			cy.getCy('account-dialog-delete-button').click();
			cy.intercept('DELETE', PlexAccountPaths.deletePlexAccountByIdEndpoint(target.id), {
				statusCode: 200, body: generateResultDTO(true),
			}).as('committedDelete');
			cy.intercept('GET', PlexAccountPaths.getAllPlexAccountsEndpoint(), {
				statusCode: 500, body: generateFailedResultDTO({ statusCode: 500 }),
			}).as('failedRefresh');

			cy.getCy('confirmation-dialog-confirmation-button').click();
			cy.wait('@committedDelete').its('response.statusCode').should('eq', 200);
			cy.wait('@failedRefresh').its('response.statusCode').should('eq', 500);
			cy.getCy('account-dialog-form').should('not.exist');
			cy.getCy(`account-card-id-${target.id}`).should('not.exist');
			cy.getCy('api-error').should('contain.text', 'HTTP 500');
			cy.getCy('close-alert-dialog').click();
			cy.getCy(`account-card-id-${retained.id}`).should('be.visible');
		});
	});

	it('Should close the confirmation dialog when the delete button is clicked and canceled', () => {
		cy.getPageData().then(({ plexAccounts }) => {
			const plexAccount = plexAccounts[1];
			if (!plexAccount) {
				throw new Error('Expected at least two accounts in test setup');
			}

			cy.getCy(`account-card-id-${plexAccount.id}`).click();

			cy.getCy('account-dialog-delete-button').click();

			cy.getCy('confirmation-dialog').should('be.visible');

			// Return the accounts without the deleted one
			cy.intercept('GET', PlexAccountPaths.getAllPlexAccountsEndpoint(), {
				statusCode: 200,
				body: generateResultDTO(plexAccounts.filter((x) => x.id !== plexAccount.id)),
			});

			cy.getCy('confirmation-dialog-cancel-button').click();

			cy.getCy('account-dialog-form').should('be.visible');
		});
	});
});
