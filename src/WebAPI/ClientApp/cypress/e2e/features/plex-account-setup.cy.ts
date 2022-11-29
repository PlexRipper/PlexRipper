import { firstValueFrom } from 'rxjs';
import { subscribeSpyTo } from '@hirez_io/observer-spy';
import { basePageSetup } from '../../fixtures/baseE2E';
import { PLEX_ACCOUNT_API_URL, PLEX_ACCOUNT_RELATIVE_PATH, PLEX_SERVER_API_URL } from '../../../src/types/const/api-urls';
import { MockConfig, generateResultDTO, generatePlexAccounts, generatePlexServers, generateSettings } from '@mock';
import { GlobalService, SignalrService } from '@service';
import { generatePlexAccountServerAndLibraries } from '@mock/mock-combination';

describe('empty spec', () => {
	beforeEach(() => {});

	it('passes', () => {
		const config: Partial<MockConfig> = {
			seed: 26398,
			plexAccountCount: 2,
			plexServerCount: 5,
		};
		basePageSetup(config);

		const { servers, libraries, account } = generatePlexAccountServerAndLibraries(config);

		cy.intercept('POST', PLEX_ACCOUNT_API_URL + '/validate', {
			statusCode: 200,
			body: generateResultDTO(account),
		});

		cy.intercept('GET', PLEX_ACCOUNT_API_URL + '/clientid', {
			statusCode: 200,
			body: generateResultDTO('RandomClientId'),
		});

		cy.intercept('GET', PLEX_ACCOUNT_API_URL + '/1', {
			statusCode: 200,
			body: generateResultDTO(account),
		});

		cy.intercept('POST', PLEX_ACCOUNT_API_URL, {
			statusCode: 200,
			body: generateResultDTO(account),
		});

		cy.intercept('GET', PLEX_SERVER_API_URL, {
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
					attemptingApplyDNSFix: false,
					retryAttemptCount: 0,
					retryAttemptIndex: 0,
					timeToNextRetry: 0,
				});
			}
			SignalrService.logState();
			SignalrService.logHistory();
		});
		expect(true).to.equal(true);
	});
});
