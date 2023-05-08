import {
	DOWNLOAD_RELATIVE_PATH,
	FOLDER_PATH_RELATIVE_PATH,
	NOTIFICATION_RELATIVE_PATH,
	NOTIFICATIONS_RELATIVE_PATH,
	PLEX_ACCOUNT_RELATIVE_PATH,
	PLEX_LIBRARY_RELATIVE_PATH,
	PLEX_SERVER_CONNECTION_RELATIVE_PATH,
	PLEX_SERVER_RELATIVE_PATH,
	PROGRESS_HUB_RELATIVE_PATH,
	SETTINGS_RELATIVE_PATH,
} from '@api-urls';
import {
	generatePlexServers,
	generatePlexLibraries,
	generateResultDTO,
	MockConfig,
	checkConfig,
	generateServerDownloadTasks,
} from '@mock';
import { generateSettingsModel } from '@factories/settings-factory';
import { generatePlexAccounts } from '@factories/plex-account-factory';
import { PlexAccountDTO, PlexLibraryDTO, PlexServerConnectionDTO, PlexServerDTO, ServerDownloadProgressDTO } from '@dto/mainApi';
import Chainable = Cypress.Chainable;

export interface IBasePageSetupResult {
	plexServers: PlexServerDTO[];
	plexServerConnections: PlexServerConnectionDTO[];
	plexLibraries: PlexLibraryDTO[];
	plexAccounts: PlexAccountDTO[];
	downloadTasks: ServerDownloadProgressDTO[];
}

export function basePageSetup(config: Partial<MockConfig> = {}): Chainable<IBasePageSetupResult> {
	const validConfig = checkConfig(config);

	// PlexServers call
	const plexServers = generatePlexServers({ config });
	cy.intercept('GET', apiRoute(PLEX_SERVER_RELATIVE_PATH), {
		statusCode: 200,
		body: generateResultDTO(plexServers),
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> plexServers', plexServers);
		}
	});

	// PlexServerConnections call
	const plexServerConnections = plexServers.flatMap((x) => x.plexServerConnections);
	cy.intercept('GET', apiRoute(PLEX_SERVER_CONNECTION_RELATIVE_PATH), {
		statusCode: 200,
		body: generateResultDTO(plexServerConnections),
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> plexServerConnections', plexServerConnections);
		}
	});

	// PlexLibraries call
	const plexLibraries = plexServers
		.map((x) =>
			generatePlexLibraries({
				config,
				plexServerId: x.id,
			}),
		)
		.flat();
	cy.intercept('GET', apiRoute(PLEX_LIBRARY_RELATIVE_PATH), {
		statusCode: 200,
		body: generateResultDTO(plexLibraries),
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> plexLibraries', plexLibraries);
		}
	});

	// PlexAccount call
	const plexAccounts = generatePlexAccounts({ config, plexServers, plexLibraries });
	cy.intercept('GET', apiRoute(PLEX_ACCOUNT_RELATIVE_PATH), {
		statusCode: 200,
		body: generateResultDTO(plexAccounts),
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> plexAccounts', plexAccounts);
		}
	});

	// DownloadTasks call
	const downloadTasks = plexServers
		.map((x) =>
			generateServerDownloadTasks({
				plexServerId: x.id,
				plexLibraryId: -1,
				config,
			}),
		)
		.flat();
	cy.intercept('GET', apiRoute(DOWNLOAD_RELATIVE_PATH), {
		statusCode: 200,
		body: generateResultDTO(downloadTasks),
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> downloadTasks', downloadTasks);
		}
	});

	// Settings call
	const settings = generateSettingsModel({ plexServers, config });
	cy.intercept('GET', apiRoute(SETTINGS_RELATIVE_PATH), {
		statusCode: 200,
		body: generateResultDTO(settings),
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> settings', settings);
		}
	});

	cy.intercept('GET', apiRoute(FOLDER_PATH_RELATIVE_PATH), {
		statusCode: 200,
		body: generateResultDTO([]),
	});

	cy.intercept('GET', apiRoute(NOTIFICATION_RELATIVE_PATH), {
		statusCode: 200,
		body: generateResultDTO([]),
	});

	cy.intercept('GET', PROGRESS_HUB_RELATIVE_PATH + '/*', {
		statusCode: 200,
		body: {},
	});

	cy.intercept('GET', NOTIFICATIONS_RELATIVE_PATH + '/*', {
		statusCode: 200,
		body: {},
	});

	return cy.wrap({
		plexServers,
		plexServerConnections,
		plexLibraries,
		plexAccounts,
		downloadTasks,
	});
}

export function route(path: string) {
	return Cypress.env('BASE_URL') + path;
}

export function apiRoute(path: string, query = '') {
	return '/api' + path + query;
}
