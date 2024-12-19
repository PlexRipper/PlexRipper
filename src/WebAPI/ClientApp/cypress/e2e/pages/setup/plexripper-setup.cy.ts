import { route } from '@fixtures/baseE2E';
import { SettingsPaths } from '@api/api-paths';

describe('PlexRipper new setup process', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 2,
			plexServerCount: 5,
		});

		// Once the setup has been completed the settings are saved
		cy.intercept('PUT', SettingsPaths.updateUserSettingsEndpoint(), {
			statusCode: 200,
		});

		cy.visit(route('/setup'));
	});

	it('Should navigate the setup process from the first to the last page by clicking the tab header navigation buttons', () => {
		cy.getPageData().then(() => {
			cy.getCy('setup-panel-1').should('be.visible');
			cy.getCy('setup-page-next-button').click();

			cy.getCy('setup-panel-2').should('be.visible');

			cy.getCy('setup-page-next-button').click();
			cy.getCy('setup-panel-3').should('be.visible');

			cy.getCy('setup-page-next-button').click();
			cy.getCy('setup-panel-4').should('be.visible');

			cy.getCy('setup-page-next-button').click();
			cy.getCy('setup-panel-5').should('be.visible');

			cy.getCy('setup-header-tab-1').click();
			cy.getCy('setup-panel-1').should('be.visible');

			cy.getCy('setup-header-tab-2').click();
			cy.getCy('setup-panel-2').should('be.visible');

			cy.getCy('setup-header-tab-3').click();
			cy.getCy('setup-panel-3').should('be.visible');

			cy.getCy('setup-header-tab-4').click();
			cy.getCy('setup-panel-4').should('be.visible');

			cy.getCy('setup-header-tab-5').click();
			cy.getCy('setup-panel-5').should('be.visible');
		});
	});
});
