import { MockConfig } from '@mock';
import { IBasePageSetupResult } from '@fixtures/baseE2E';

declare global {
	namespace Cypress {
		interface Chainable {
			/**
			 * Custom command to set up the base page request interceptions for e2e tests
			 * @example cy.basePageSetup({ plexAccountCount: 2, plexServerCount: 5 })
			 */
			basePageSetup(config: Partial<MockConfig>): Chainable<IBasePageSetupResult>;
		}
	}
}
