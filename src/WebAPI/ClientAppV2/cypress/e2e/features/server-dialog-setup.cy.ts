import { cy, describe, it, before, beforeEach, after } from 'local-cypress';
import { route, basePageSetup } from '@fixtures/baseE2E';

describe('PlexRipper Server Dialog', () => {
	beforeEach(() => {
		const config = {
			plexAccountCount: 2,
			plexServerCount: 5,
			plexLibraryCount: 5,
		};

		basePageSetup(config);

		cy.visit(route('/')).as('setupPage');
	});

	it('Should navigate the setup process from the first to the last page when the navigation buttons are used', () => {
		cy.get('[data-cy="server-dialog-2"]').click();
		cy.get('[data-cy="server-dialog-tab-1"]').click();
		cy.get('[data-cy="server-dialog-tab-2"]').click();
		cy.get('[data-cy="server-dialog-tab-3"]').click();
		cy.get('[data-cy="server-dialog-tab-4"]').click();
		cy.get('[data-cy="server-dialog-tab-5"]').click();

		cy.url().should('eq', route('/'));
	});
});
