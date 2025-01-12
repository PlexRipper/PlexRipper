import { route } from '@fixtures';
import { AuthenticationPaths } from '@api/api-paths';
import { generateResultDTO } from '@mock';
import type { AppCredentialsDTO } from '@dto';
import { randPassword } from '@ngneat/falso';

describe('PlexRipper new setup process', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 0,
			plexServerCount: 0,
			invalidDefaultFolderPaths: true,
		});

		cy.visit(route('/setup'));
	});

	function areTabsActive(active: number, max: number) {
		for (let i = 1; i <= 6; i++) {
			if (i === active) {
				cy.getCy(`setup-header-tab-${i}`).should('have.class', 'q-tab--active');
			}
			if (i <= max) {
				cy.getCy(`setup-header-tab-${i}`).should('not.be.disabled');
			} else {
				cy.getCy(`setup-header-tab-${i}`).should('have.class', 'disabled');
			}
		}
	}

	it('Should navigate the setup process from the first to the last page', () => {
		cy.getPageData().then(({ settings }) => {
			// Disclaimer
			cy.getCy('setup-panel-1').should('be.visible');
			areTabsActive(1, 1);

			cy.getCy('setup-disclaimer-accept-button').click();

			// Introduction
			cy.getCy('setup-panel-2').should('be.visible');
			areTabsActive(2, 3);
			cy.getCy('setup-page-next-button').click();

			// Authorization
			cy.getCy('setup-panel-3').should('be.visible');
			areTabsActive(3, 3);
			cy.getCy('setup-page-next-button').should('be.disabled');

			const newUsername = 'PlexRipperRocks123';
			const newPassword = 'x!WCF*bnT$FEu6';
			cy.getCy('app-username-input').clear();
			cy.getCy('app-username-input').type(newUsername);

			cy.getCy('app-password-input').clear();
			cy.getCy('app-password-input').type(newPassword);

			cy.getCy('app-confirm-password-input').clear();
			cy.getCy('app-confirm-password-input').type(newPassword);

			cy.intercept('GET', AuthenticationPaths.getAppCredentials(), {
				statusCode: 200,
				body: generateResultDTO<AppCredentialsDTO>({
					isDefaultCredentials: false,
					userName: newUsername,
					password: randPassword({ size: 16 }) + '$%&',
				}),
			});
			cy.getCy('save-credentials-button').click();
			cy.getCy('setup-page-next-button').click();

			// Folder paths check
			cy.getCy('setup-page-next-button').should('be.disabled');
			areTabsActive(4, 4);
			cy.getCy('setup-panel-4').should('be.visible');

			cy.correctDefaultFolderPaths();

			// Plex Accounts
			cy.getCy('setup-page-next-button').click();
			areTabsActive(5, 5);
			cy.getCy('setup-panel-5').should('be.visible');

			cy.getCy('setup-page-next-button').should('be.disabled');

			cy.createPlexAccount(null);

			// Finish page
			cy.getCy('setup-page-next-button').click();
			areTabsActive(6, 6);
			cy.getCy('setup-panel-6').should('be.visible');

			cy.getCy('finish-setup-links').should('be.visible');
			cy.getCy('link-list-item-0-link').should('be.visible');
			cy.getCy('link-list-item-1-link').should('be.visible');
			cy.getCy('link-list-item-2-link').should('be.visible');
			cy.getCy('link-list-item-3-link').should('be.visible');

			// Use left navigation tabs
			cy.getCy('setup-header-tab-1').click();
			cy.getCy('setup-panel-1').should('be.visible');
			cy.getCy('setup-disclaimer-accept-button').click();

			cy.getCy('setup-header-tab-2').click();
			cy.getCy('setup-panel-2').should('be.visible');

			cy.getCy('setup-header-tab-3').click();
			cy.getCy('setup-panel-3').should('be.visible');

			cy.getCy('setup-header-tab-4').click();
			cy.getCy('setup-panel-4').should('be.visible');

			cy.getCy('setup-header-tab-5').click();
			cy.getCy('setup-panel-5').should('be.visible');

			cy.getCy('setup-header-tab-6').click();
			cy.getCy('setup-panel-6').should('be.visible');

			// Ensure disclaimer is sent to the server
			cy.awaitSettingsUpdate().then(() => {
				expect(settings.generalSettings.firstTimeSetup).to.eq(false);
				expect(settings.generalSettings.hasAgreedToDisclaimer).to.eq(true);
			});

			cy.getCy('setup-page-skip-setup-button').click();
			cy.url().should('eq', route('/'));
		});
	});
});
