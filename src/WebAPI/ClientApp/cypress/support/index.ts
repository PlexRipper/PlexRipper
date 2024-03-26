import type { IBasePageSetupResult } from '@fixtures/baseE2E';
import type { MockConfig } from '@mock';

declare global {
	namespace Cypress {
		// 🤔 unsure why this Subject is unused, nor what to do with it...
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		interface Chainable<Subject = any> {
			/**
			 * Custom command to set up the base page request interceptions for e2e tests
			 * @example cy.basePageSetup({ plexAccountCount: 2, plexServerCount: 5 })
			 */
			basePageSetup(config: Partial<MockConfig>): Chainable<IBasePageSetupResult>;

			getPageData(): Chainable<IBasePageSetupResult>;

			visitEmptyPage(): Chainable;

			getCy<E extends Node = HTMLElement>(
				selector: string,
				options?: Partial<Loggable & Timeoutable & Withinable & Shadow>,
			): Chainable<JQuery<E>>;
		}
	}
}
