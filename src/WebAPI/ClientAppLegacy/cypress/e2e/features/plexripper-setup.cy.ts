import { basePageSetup } from '@fixtures/baseE2E';
import { cy, describe, it } from 'local-cypress';

describe('empty spec', () => {
	it('passes', () => {
		const config = {
			plexAccountCount: 2,
			plexServerCount: 5,
		};

		basePageSetup(config);

		cy.visit('/setup').as('setupPage');
		cy.wait(1000);
		// cy.waitUntil(async () => await firstValueFrom(GlobalService.getPageSetupReady()));
		cy.get('[data-cy="setup-page-next-button"]').click();
	});
});
