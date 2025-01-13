import { route } from '@fixtures';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import type { GeneratePlexTokenResponse, PlexAccountDTO } from '@dto';
import { PlexAccountPaths } from '@api-urls';

describe('Add Plex account to PlexRipper', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 2,
			plexServerCount: 5,
		});

		cy.visit(route('/settings/accounts'));
	});

	it('Should generate and copy a plex token when the plex account has an username and password', () => {
		cy.getPageData().then(({ plexAccounts }) => {
			const account: PlexAccountDTO = plexAccounts[0];

			cy.getCy('account-card-id-' + account.id).click();

			cy.intercept('GET', PlexAccountPaths.generatePlexTokenEndpoint(account.id, { verificationCode: '' }), {
				statusCode: 200,
				body: generateResultDTO({
					plexAuthToken: 'some-plex-api-token',
				} as GeneratePlexTokenResponse),
			});

			cy.getCy('account-dialog-generate-token-button').click();

			cy.getCy('generate-token-dialog-copy-button').click();
		});
	});

	it('Should generate and copy a plex token when the plex account has an username, password and is 2FA', () => {
		cy.getPageData().then(({ plexAccounts }) => {
			const account: PlexAccountDTO = plexAccounts[0];

			cy.getCy('account-card-id-' + account.id).click();

			cy.intercept('GET', PlexAccountPaths.generatePlexTokenEndpoint(account.id, { verificationCode: '' }), {
				statusCode: 200,
				body: generateFailedResultDTO({
					errors: [
						{
							reasons: [],
							message: 'Please enter the verification code',
							metadata: {},
						},
					],
				}),
			});
			// Attempt 1
			cy.getCy('account-dialog-generate-token-button').click();

			cy.getCy('generate-token-dialog').should('be.visible');

			cy.getCy('2fa-code-verification-input').should('exist');

			cy.intercept('GET', PlexAccountPaths.generatePlexTokenEndpoint(account.id, { verificationCode: '123456' }), {
				statusCode: 200,
				body: generateResultDTO({
					plexAuthToken: 'some-plex-api-token',
				} as GeneratePlexTokenResponse),
			});

			cy.get(':nth-child(1) > [data-test="single-input"]').type('123456');

			cy.getCy('generate-token-dialog-token-input').should('have.value', 'some-plex-api-token');

			cy.getCy('generate-token-dialog-copy-button').click();

			cy.getCy('generate-token-dialog-hide-button').click();
		});
	});
});
