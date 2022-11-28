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
import { checkConfig, generateResultDTO, generateSettings, MockConfig } from '@mock';

export function basePageSetup(config: Partial<MockConfig> = {}) {
	const configValid = checkConfig(config);

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
}
