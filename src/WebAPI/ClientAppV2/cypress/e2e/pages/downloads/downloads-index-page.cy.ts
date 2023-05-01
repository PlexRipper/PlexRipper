import { cy, describe, it, before, beforeEach, after } from 'local-cypress';
import { route, basePageSetup } from '@fixtures/baseE2E';

describe('PlexRipper new setup process', () => {
	beforeEach(() => {
		basePageSetup({
			plexAccountCount: 2,
			plexServerCount: 5,
			plexLibraryCount: 5,
			tvShowDownloadTask: 5,
		});

		cy.visit(route('/downloads')).as('downloadsPage');
	});

	it('Should navigate the setup process from the first to the last page when the navigation buttons are used', () => {
		cy.url().should('eq', route('/downloads'));
	});
});
