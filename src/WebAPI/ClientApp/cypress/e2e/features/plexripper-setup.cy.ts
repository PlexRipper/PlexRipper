import { firstValueFrom } from 'rxjs';
import { MockConfig, generateResultDTO, generatePlexAccounts, generatePlexServers, generateSettings } from '@mock';
import {
	DOWNLOAD_API_URL,
	FOLDER_PATH_API_URL,
	NOTIFICATION_API_URL,
	NOTIFICATIONS_HUB_URL,
	PLEX_ACCOUNT_API_URL,
	PLEX_LIBRARY_API_URL,
	PLEX_SERVER_API_URL,
	PROGRESS_HUB_URL,
	SETTINGS_API_URL,
} from '@api-urls';
import { GlobalService } from '@service';

describe('empty spec', () => {
	beforeEach(() => {});

	it('passes', () => {
		const config: MockConfig = {
			plexAccountCount: 2,
			plexServerCount: 5,
		};

		const plexServers = generatePlexServers(config);
		cy.intercept('GET', PLEX_SERVER_API_URL, {
			statusCode: 200,
			body: generateResultDTO(plexServers),
		});

		cy.intercept('GET', DOWNLOAD_API_URL, {
			statusCode: 200,
			body: generateResultDTO([]),
		});

		cy.intercept('GET', FOLDER_PATH_API_URL, {
			statusCode: 200,
			body: generateResultDTO([]),
		});

		cy.intercept('GET', PLEX_LIBRARY_API_URL, {
			statusCode: 200,
			body: generateResultDTO([]),
		});

		cy.intercept('GET', PLEX_ACCOUNT_API_URL, {
			statusCode: 200,
			body: generateResultDTO(generatePlexAccounts(config, plexServers, [])),
		});

		cy.intercept('GET', SETTINGS_API_URL, {
			statusCode: 200,
			body: generateResultDTO(generateSettings(config)),
		});

		cy.intercept('GET', NOTIFICATION_API_URL, {
			statusCode: 200,
			body: generateResultDTO([]),
		});

		cy.intercept('GET', PROGRESS_HUB_URL + '/*', {
			statusCode: 200,
			body: {},
		});

		cy.intercept('GET', NOTIFICATIONS_HUB_URL + '/*', {
			statusCode: 200,
			body: {},
		});

		cy.visit('/setup').as('setupPage');
		cy.wait(1000);
		// cy.waitUntil(async () => await firstValueFrom(GlobalService.getPageSetupReady()));
		cy.get('[data-cy="setup-page-next-button"]').click();
		expect(true).to.equal(true);
	});
});
