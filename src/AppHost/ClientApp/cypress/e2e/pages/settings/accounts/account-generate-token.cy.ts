import { route } from '@fixtures';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import type { GeneratePlexTokenResponse, PlexAccountDTO } from '@dto';
import { PlexAccountPaths } from '@api-urls';

describe('Add Plex account to Reaparr', () => {
	beforeEach(() => {
		cy.basePageSetup({
			plexAccountCount: 2,
			plexServerCount: 5,
		});

		cy.visit(route('/settings/accounts'));
	});

	it('Should not show the generate token button when the plex account was created with a token', () => {
		cy.basePageSetup({
			plexAccountCount: 1,
			plexServerCount: 5,
			override: {
				plexAccounts: (plexAccounts) => {
					plexAccounts[0]!.customAuthenticationToken = 'some-custom-auth-token';
					plexAccounts[0]!.authenticationToken = '';
					plexAccounts[0]!.username = '';
					plexAccounts[0]!.password = '';
					return plexAccounts;
				},
			},
		});

		cy.visit(route('/settings/accounts'));

		cy.getPageData().then(({ plexAccounts }) => {
			const account: PlexAccountDTO = plexAccounts[0]!;

			cy.getCy('account-card-id-' + account.id).click();

			cy.getCy('account-dialog-generate-token-button').should('not.exist');
		});
	});

	it('Should generate and copy a plex token when the plex account has an username and password', () => {
		cy.getPageData().then(({ plexAccounts }) => {
			const account: PlexAccountDTO = plexAccounts[0]!;

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

	it('Should show a token generation failure and recover after hiding and reopening the dialog', () => {
		cy.getPageData().then(({ plexAccounts }) => {
			const account: PlexAccountDTO = plexAccounts[0]!;
			const endpoint = PlexAccountPaths.generatePlexTokenEndpoint(account.id, { verificationCode: '' });
			const errorMessage = 'Plex token generation failed';

			cy.getCy('account-card-id-' + account.id).click();

			cy.intercept({ method: 'GET', url: endpoint, times: 1 }, {
				statusCode: 500,
				body: generateFailedResultDTO({
					statusCode: 500,
					errors: [{ message: errorMessage, reasons: [], metadata: {} }],
				}),
			}).as('generateTokenFailure');

			cy.getCy('account-dialog-generate-token-button').click();
			cy.wait('@generateTokenFailure').its('response.statusCode').should('eq', 500);
			cy.getCy('alert-dialog').should('be.visible').and('contain.text', errorMessage);
			cy.getCy('close-alert-dialog').click();
			cy.getCy('alert-dialog').should('not.exist');
			cy.getCy('generate-token-error').should('be.visible').and('contain.text', errorMessage);
			cy.getCy('generate-token-dialog').should('be.visible');
			cy.getCy('generate-token-dialog-token-input').should('be.visible').and('have.value', '');

			cy.getCy('generate-token-dialog-hide-button').click();
			cy.getCy('generate-token-dialog').should('not.exist');

			cy.intercept({ method: 'GET', url: endpoint, times: 1 }, {
				statusCode: 200,
				body: generateResultDTO<GeneratePlexTokenResponse>({
					isUnAuthorized: false,
					needsVerificationCode: false,
					plexAuthToken: 'retried-plex-token',
				}),
			}).as('generateTokenRetry');

			cy.getCy('account-dialog-generate-token-button').click();
			cy.wait('@generateTokenRetry').its('response.statusCode').should('eq', 200);
			cy.getCy('generate-token-dialog-token-input').should('have.value', 'retried-plex-token');
			cy.getCy('generate-token-error').should('not.exist');
		});
	});

	it('Should generate and copy a plex token when the plex account has an username, password and is 2FA', () => {
		cy.getPageData().then(({ plexAccounts }) => {
			const account: PlexAccountDTO = plexAccounts[0]!;

			cy.getCy('account-card-id-' + account.id).click();

			cy.intercept('GET', PlexAccountPaths.generatePlexTokenEndpoint(account.id, { verificationCode: '' }), {
				statusCode: 200,
				body: generateResultDTO<GeneratePlexTokenResponse>({
					isUnAuthorized: false,
					needsVerificationCode: true,
					plexAuthToken: '',
				}),
			});
			// Attempt 1
			cy.getCy('account-dialog-generate-token-button').click();

			cy.getCy('generate-token-dialog').should('be.visible');

			cy.getCy('2fa-code-verification-input').should('exist');

			cy.intercept('GET', PlexAccountPaths.generatePlexTokenEndpoint(account.id, { verificationCode: '123456' }), {
				statusCode: 200,
				body: generateResultDTO<GeneratePlexTokenResponse>({
					isUnAuthorized: false,
					needsVerificationCode: false,
					plexAuthToken: 'some-plex-api-token',
				}),
			});

			cy.getCy('2fa-code-verification-input')
				.find('[data-test="single-input"]')
				.each((el, i) => {
					cy.wrap(el).type('123456'[i]!);
				});

			cy.getCy('generate-token-dialog-token-input').should('have.value', 'some-plex-api-token');

			cy.getCy('generate-token-dialog-copy-button').click();

			cy.getCy('generate-token-dialog-hide-button').click();
		});
	});
});
