import { cy } from 'local-cypress';
import {
	DOWNLOAD_API_URL,
	FOLDER_PATH_API_URL,
	NOTIFICATION_API_URL,
	NOTIFICATIONS_HUB_URL,
	PLEX_ACCOUNT_API_URL,
	PLEX_LIBRARY_API_URL,
	PLEX_SERVER_API_URL,
	PLEX_SERVER_CONNECTION_API_URL,
	PROGRESS_HUB_URL,
	SETTINGS_API_URL,
} from '@api-urls';
import { checkConfig, generatePlexServers, generateResultDTO, generateSettings, MockConfig } from '@mock';

export function basePageSetup(config: Partial<MockConfig> = {}) {
	const configValid = checkConfig(config);

	const plexServers = generatePlexServers(configValid);
	const plexServerConnections = plexServers.flatMap((x) => x.plexServerConnections);

	cy.intercept('GET', PLEX_SERVER_API_URL, {
		statusCode: 200,
		body: generateResultDTO([]),
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
		body: generateResultDTO([]),
	});

	cy.intercept('GET', SETTINGS_API_URL, {
		statusCode: 200,
		body: generateResultDTO(generateSettings(configValid)),
	});

	cy.intercept('GET', NOTIFICATION_API_URL, {
		statusCode: 200,
		body: generateResultDTO([]),
	});

	cy.intercept('GET', PLEX_SERVER_CONNECTION_API_URL, {
		statusCode: 200,
		body: generateResultDTO(plexServerConnections),
	});

	cy.intercept('GET', PROGRESS_HUB_URL + '/*', {
		statusCode: 200,
		body: {},
	});

	cy.intercept('GET', NOTIFICATIONS_HUB_URL + '/*', {
		statusCode: 200,
		body: {},
	});
}
