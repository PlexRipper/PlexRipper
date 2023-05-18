import { apiRoute, APIRoute, route } from '@fixtures/baseE2E';

describe('PlexRipper new setup process', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 2,
			plexServerCount: 5,
		});

		// Once the setup has been completed the settings are saved
		cy.intercept('PUT', apiRoute({ type: APIRoute.Settings }), {
			statusCode: 200,
		});

		cy.visit(route('/setup')).as('setupPage');
	});

	it('Should navigate the setup process from the first to the last page when the navigation buttons are used', () => {
		cy.getCy('setup-page-next-button').click();

		cy.getCy('setup-page-next-button').click();

		cy.getCy('setup-page-next-button').click();

		cy.getCy('setup-page-next-button').click();

		cy.getCy('setup-page-skip-setup-button').click();

		cy.url().should('eq', route('/'));
	});

	it('Should navigate the setup process from the first to the last page by clicking the tab header navigation buttons', () => {
		cy.getCy('setup-header-tab-1').click();

		cy.getCy('setup-header-tab-2').click();

		cy.getCy('setup-header-tab-3').click();

		cy.getCy('setup-header-tab-4').click();

		cy.getCy('setup-header-tab-5').click();

		cy.getCy('setup-page-skip-setup-button').click();

		cy.url().should('eq', route('/'));
	});
});
