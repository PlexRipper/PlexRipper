import { route } from '@fixtures/baseE2E';
import { generateResultDTO } from '@mock';
import { PlexAccountPaths } from '@api-urls';

describe('Delete Plex account from PlexRipper', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 2,
			plexServerCount: 5,
		});

		cy.visit(route('/settings/accounts')).as('setupPage');
	});

	it('Should delete a Plex account when the delete button is clicked and confirmed', () => {
		cy.getPageData().then(({ plexAccounts }) => {
			const plexAccount = plexAccounts[1];

			cy.getCy(`account-card-id-${plexAccount.id}`).click();

			cy.getCy('account-dialog-delete-button').click();

			cy.getCy('confirmation-dialog').should('be.visible');

			// Delete Action
			cy.intercept('DELETE', PlexAccountPaths.deletePlexAccountByIdEndpoint(plexAccount.id), {
				statusCode: 200,
			});

			// Return the accounts without the deleted one
			cy.intercept('GET', PlexAccountPaths.getAllPlexAccountsEndpoint(), {
				statusCode: 200,
				body: generateResultDTO(plexAccounts.filter((x) => x.id !== plexAccount.id)),
			});

			cy.getCy('confirmation-dialog-confirmation-button').click();

			cy.getCy('account-dialog-form').should('not.be.visible');

			cy.getCy(`account-card-id-${plexAccount.id}`).should('not.be.visible');
		});
	});

	it('Should close the confirmation dialog when the delete button is clicked and canceled', () => {
		cy.getPageData().then(({ plexAccounts }) => {
			const plexAccount = plexAccounts[1];

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
