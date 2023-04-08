import { cy, describe, it, before, beforeEach, after } from 'local-cypress';
import route, { basePageSetup } from '@fixtures/baseE2E';
import { SETTINGS_API_URL } from '@api-urls';

describe('PlexRipper new setup process', () => {
	beforeEach(() => {
		const config = {
			plexAccountCount: 2,
			plexServerCount: 5,
		};

		basePageSetup(config);

		// Once the setup has been completed the settings are saved
		cy.intercept('PUT', SETTINGS_API_URL, {
			statusCode: 200,
		});

		cy.visit(route('/setup')).as('setupPage');
	});

	it('Should navigate the setup process from the first to the last page when the navigation buttons are used', () => {
		cy.get('[data-cy="setup-page-next-button"]').click();

		cy.get('[data-cy="setup-page-next-button"]').click();

		cy.get('[data-cy="setup-page-next-button"]').click();

		cy.get('[data-cy="setup-page-next-button"]').click();

		cy.get('[data-cy="setup-page-skip-setup-button"]').click();

		cy.url().should('eq', route('/'));
	});

	it('Should navigate the setup process from the first to the last page by clicking the tab header navigation buttons', () => {
		cy.get('[data-cy="setup-header-tab-1"]').click();

		cy.get('[data-cy="setup-header-tab-2"]').click();

		cy.get('[data-cy="setup-header-tab-3"]').click();

		cy.get('[data-cy="setup-header-tab-4"]').click();

		cy.get('[data-cy="setup-header-tab-5"]').click();

		cy.get('[data-cy="setup-page-skip-setup-button"]').click();

		cy.url().should('eq', route('/'));
	});
});
