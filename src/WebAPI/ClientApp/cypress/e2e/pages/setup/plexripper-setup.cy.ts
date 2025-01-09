import { route } from '@fixtures';
import { AuthenticationPaths, SettingsPaths } from '@api/api-paths';
import { generateResultDTO } from '@mock';
import type { AppCredentialsDTO } from '@dto';
import { randPassword } from '@ngneat/falso';

describe('PlexRipper new setup process', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 0,
			plexServerCount: 0,
		});

		// Once the setup has been completed the settings are saved
		cy.intercept('PUT', SettingsPaths.updateUserSettingsEndpoint(), {
			statusCode: 200,
		});

		cy.visit(route('/setup'));
	});

	it('Should navigate the setup process from the first to the last page', () => {
		cy.getPageData().then(() => {
			cy.getCy('setup-panel-1').should('be.visible');
			cy.getCy('setup-disclaimer-accept-button').click();

			// Introduction
			cy.getCy('setup-panel-2').should('be.visible');

			cy.getCy('setup-page-next-button').click();

			// Authorization
			const newUsername = 'PlexRipperRocks123';
			const newPassword = 'x!WCF*bnT$FEu6';
			cy.getCy('setup-panel-3').should('be.visible');
			cy.getCy('setup-page-next-button').should('be.disabled');

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

			// Folder paths check
			cy.getCy('setup-page-next-button').click();
			cy.getCy('setup-panel-4').should('be.visible');

			// Plex Accounts
			cy.getCy('setup-page-next-button').click();
			cy.getCy('setup-panel-5').should('be.visible');
			cy.getCy('setup-page-next-button').should('be.disabled');

			cy.createPlexAccount(null);

			// Finish page
			cy.getCy('setup-page-next-button').click();
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

			cy.getCy('setup-page-skip-setup-button').click();
			cy.url().should('eq', route('/'));
		});
	});
});
