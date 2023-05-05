import { route, basePageSetup } from '@fixtures/baseE2E';
import { PLEX_ACCOUNT_RELATIVE_PATH, PLEX_SERVER_RELATIVE_PATH, SETTINGS_RELATIVE_PATH } from '@api-urls';
import { generateResultDTO } from '@mock';
import { SignalrService } from '@service';
import { PlexServerConnectionDTO } from '@dto/mainApi';

describe('empty spec', () => {
	beforeEach(() => {
		const config = {
			plexAccountCount: 2,
			plexServerCount: 5,
		};

		basePageSetup(config);

		// Once the setup has been completed the settings are saved
		cy.intercept('PUT', SETTINGS_RELATIVE_PATH, {
			statusCode: 200,
		});

		cy.visit(route('/setup')).as('setupPage');
	});

	it('passes', () => {
		const config = {
			seed: 26398,
			plexAccountCount: 2,
			plexServerCount: 5,
		};
		basePageSetup(config);

		cy.intercept('POST', PLEX_ACCOUNT_RELATIVE_PATH + '/validate', {
			statusCode: 200,
			body: generateResultDTO(account),
		});

		cy.intercept('GET', PLEX_ACCOUNT_RELATIVE_PATH + '/clientid', {
			statusCode: 200,
			body: generateResultDTO('RandomClientId'),
		});

		cy.intercept('GET', PLEX_ACCOUNT_RELATIVE_PATH + '/1', {
			statusCode: 200,
			body: generateResultDTO(account),
		});

		cy.intercept('POST', PLEX_ACCOUNT_RELATIVE_PATH, {
			statusCode: 200,
			body: generateResultDTO(account),
		});

		cy.intercept('GET', PLEX_SERVER_RELATIVE_PATH, {
			statusCode: 200,
			body: generateResultDTO(servers),
		});

		cy.visit('/setup').as('setupPage');
		// Go to future plans page
		cy.get('[data-cy="setup-page-next-button"]').click();
		// Go to checking paths page
		cy.get('[data-cy="setup-page-next-button"]').click();
		// Go to Plex Accounts page
		cy.get('[data-cy="setup-page-next-button"]').click();
		// Add new account
		cy.get('[data-cy="account-overview-add-account"]').click();

		// Fill in credentials
		cy.get('[data-cy="account-form-display-name-input"]').type(account.displayName);
		cy.get('[data-cy="account-form-username-input"]').type(account.username);
		cy.get('[data-cy="account-form-password-input"]').type(account.password);

		// Validate Action
		cy.get('[data-cy="account-dialog-validate-button"]').click();
		// Create Action
		cy.get('[data-cy="account-dialog-create-button"]').click();
		// Confirm the setup progress is visible
		cy.get('.account-setup-progress').then(() => {
			const accessibleServerIds = account.plexServerAccess.map((x) => x.plexServerId);
			const accessibleServers = servers.filter((x) => accessibleServerIds.includes(x.id));

			for (const accessibleServer of accessibleServers) {
				SignalrService.setInspectServerProgress({
					plexServerId: accessibleServer.id,
					completed: true,
					connectionSuccessful: true,
					message: '',
					statusCode: 200,
					retryAttemptCount: 0,
					plexServerConnection: {} as PlexServerConnectionDTO,
					retryAttemptIndex: 0,
					timeToNextRetry: 0,
				});
			}
			SignalrService.logState();
			SignalrService.logHistory();
		});
	});
});
