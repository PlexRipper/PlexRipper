import { basePageSetup } from '@fixtures/baseE2E';
import { cy, describe, it, Cypress } from 'local-cypress';
import { SETTINGS_API_URL } from '@api-urls';

describe('PlexRipper new setup process', () => {
	it('Should navigate the setup process from the first to the last page when the navigation buttons are used', () => {
		const config = {
			plexAccountCount: 2,
			plexServerCount: 5,
		};

		basePageSetup(config);

		// Once the setup has been completed the settings are saved
		cy.intercept('PUT', SETTINGS_API_URL, {
			statusCode: 200,
		});

		cy.visit('/setup').as('setupPage');

		cy.get('[data-cy="setup-page-next-button"]').click();

		cy.get('[data-cy="setup-page-next-button"]').click();

		cy.get('[data-cy="setup-page-next-button"]').click();

		cy.get('[data-cy="setup-page-next-button"]').click();

		cy.get('[data-cy="setup-page-skip-setup-button"]').click();

		cy.url().should('eq', Cypress.config().baseUrl + '/');
	});

	it('Should navigate the setup process from the first to the last page by clicking the tab header navigation buttons', () => {
		const config = {
			plexAccountCount: 2,
			plexServerCount: 5,
		};

		basePageSetup(config);

		// Once the setup has been completed the settings are saved
		cy.intercept('PUT', SETTINGS_API_URL, {
			statusCode: 200,
		});

		cy.visit('/setup').as('setupPage');

		cy.get('[data-cy="setup-header-tab-1"]').click();

		cy.get('[data-cy="setup-header-tab-2"]').click();

		cy.get('[data-cy="setup-header-tab-3"]').click();

		cy.get('[data-cy="setup-header-tab-4"]').click();

		cy.get('[data-cy="setup-header-tab-5"]').click();

		cy.get('[data-cy="setup-page-skip-setup-button"]').click();

		cy.url().should('eq', Cypress.config().baseUrl + '/');
	});
});
