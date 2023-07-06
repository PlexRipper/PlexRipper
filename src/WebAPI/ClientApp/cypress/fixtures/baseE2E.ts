import {
	DOWNLOAD_RELATIVE_PATH,
	FOLDER_PATH_RELATIVE_PATH,
	NOTIFICATION_RELATIVE_PATH,
	NOTIFICATIONS_RELATIVE_PATH,
	PLEX_ACCOUNT_RELATIVE_PATH,
	PLEX_LIBRARY_RELATIVE_PATH,
	PLEX_MEDIA_RELATIVE_PATH,
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
	generatePlexMedias,
} from '@mock';
import { generateSettingsModel } from '@factories/settings-factory';
import { generatePlexAccounts } from '@factories/plex-account-factory';
import {
	PlexAccountDTO,
	PlexLibraryDTO,
	PlexMediaSlimDTO,
	PlexMediaType,
	PlexServerConnectionDTO,
	PlexServerDTO,
	ServerDownloadProgressDTO,
	SettingsModelDTO,
} from '@dto/mainApi';
import Chainable = Cypress.Chainable;

export interface IBasePageSetupResult {
	plexServers: PlexServerDTO[];
	plexServerConnections: PlexServerConnectionDTO[];
	plexLibraries: PlexLibraryDTO[];
	plexAccounts: PlexAccountDTO[];
	serverDownloadProgress: ServerDownloadProgressDTO[];
	mediaData: {
		libraryId: number;
		media: PlexMediaSlimDTO[];
	}[];
	settings: SettingsModelDTO;
	config: MockConfig;
}

export function basePageSetup(config: Partial<MockConfig> = {}): Chainable<IBasePageSetupResult> {
	const validConfig = checkConfig(config);

	// PlexServers call
	const plexServers = generatePlexServers({ config });
	cy.intercept('GET', apiRoute({ type: APIRoute.PlexServer }), {
		statusCode: 200,
		body: generateResultDTO(plexServers),
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> plexServers', plexServers);
		}
	});

	// PlexServerConnections call
	const plexServerConnections = plexServers.flatMap((x) => x.plexServerConnections);
	cy.intercept('GET', apiRoute({ type: APIRoute.PlexServerConnection }), {
		statusCode: 200,
		body: generateResultDTO(plexServerConnections),
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> plexServerConnections', plexServerConnections);
		}
	});

	// PlexLibraries call
	const plexLibraries = plexServers
		.map((x) => {
			return [
				...generatePlexLibraries({
					type: PlexMediaType.Movie,
					config,
					plexServerId: x.id,
				}),
				...generatePlexLibraries({
					type: PlexMediaType.TvShow,
					config,
					plexServerId: x.id,
				}),
			];
		})
		.flat();
	cy.intercept('GET', apiRoute({ type: APIRoute.PlexLibrary }), {
		statusCode: 200,
		body: generateResultDTO(plexLibraries),
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> plexLibraries', plexLibraries);
		}
	});

	// Detail call for every library
	for (const library of plexLibraries) {
		cy.intercept(
			'GET',
			apiRoute({
				type: APIRoute.PlexLibrary,
				path: `/${library.id}`,
			}),
			{
				statusCode: 200,
				body: generateResultDTO(library),
			},
		);
	}

	// PlexAccount call
	const plexAccounts = generatePlexAccounts({ config, plexServers, plexLibraries });
	cy.intercept('GET', apiRoute({ type: APIRoute.PlexAccount }), {
		statusCode: 200,
		body: generateResultDTO(plexAccounts),
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> plexAccounts', plexAccounts);
		}
	});

	// DownloadTasks call
	const serverDownloadProgress = plexServers
		.map((x) =>
			generateServerDownloadTasks({
				plexServerId: x.id,
				plexLibraryId: -1,
				config,
			}),
		)
		.flat();
	cy.intercept('GET', apiRoute({ type: APIRoute.Download }), {
		statusCode: 200,
		body: generateResultDTO(serverDownloadProgress),
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> downloadTasks', serverDownloadProgress);
		}
	});

	// Settings call
	const settings = generateSettingsModel({ plexServers, config });
	cy.intercept('GET', apiRoute({ type: APIRoute.Settings }), {
		statusCode: 200,
		body: generateResultDTO(settings),
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> settings', settings);
		}
	});

	// Generate library media page data
	const mediaData: {
		libraryId: number;
		media: PlexMediaSlimDTO[];
	}[] = [];
	for (const library of plexLibraries) {
		if (library.type !== PlexMediaType.Movie) {
			continue;
		}
		const mediaList = generatePlexMedias({
			plexLibraryId: library.id,
			plexServerId: library.plexServerId,
			type: library.type,
			config,
		});

		mediaData.push({
			libraryId: library.id,
			media: mediaList,
		});

		// Intercept the Library media call
		cy.intercept(
			'GET',
			apiRoute({
				type: APIRoute.PlexMedia,
				path: `/library/${library.id}`,
				wildcard: false,
				query: {
					page: '*',
					size: '*',
				},
			}),
			{
				statusCode: 200,
				body: generateResultDTO(mediaList),
			},
		);
	}

	cy.intercept('GET', apiRoute({ type: APIRoute.FolderPath }), {
		statusCode: 200,
		body: generateResultDTO([]),
	});

	cy.intercept('GET', apiRoute({ type: APIRoute.Notification }), {
		statusCode: 200,
		body: generateResultDTO([]),
	});

	cy.intercept('GET', apiRoute({ type: APIRoute.ProgressHub, excludeApiPrefix: true }), {
		statusCode: 200,
		body: {},
	});

	cy.intercept('GET', apiRoute({ type: APIRoute.NotificationsHub, excludeApiPrefix: true }), {
		statusCode: 200,
		body: {},
	});

	// Correct library data
	for (const library of plexLibraries) {
		const mediaList = mediaData.find((x) => x.libraryId === library.id)?.media ?? [];
		if (mediaList.length) {
			library.mediaSize = mediaList.reduce((acc, x) => acc + x.mediaSize, 0);
			library.count = mediaList.length;
		}
	}

	return cy.wrap({
		config: validConfig,
		plexServers,
		plexServerConnections,
		plexLibraries,
		plexAccounts,
		serverDownloadProgress,
		mediaData,
		settings,
	});
}

export function route(path: string) {
	return Cypress.env('BASE_URL') + path;
}

export function apiRoute({
	type,
	path = '',
	query = {},
	wildcard = true,
	excludeApiPrefix = false,
}: {
	type: APIRoute;
	path?: string;
	query?: Record<string, string>;
	wildcard?: boolean;
	excludeApiPrefix?: boolean;
}): string {
	const fullPath = `${!excludeApiPrefix ? '/api' : ''}${type}${path}`;
	const urlBuilder = new URL(fullPath, 'http://localhost.com');

	for (const param of Object.keys(query).map((x) => [x, query[x]])) {
		urlBuilder.searchParams.append(param[0], param[1]);
	}

	let url = urlBuilder.toString().replace('http://localhost.com', '');
	if (wildcard) {
		url += '*';
	}
	return url;
}

export enum APIRoute {
	PlexServer = PLEX_SERVER_RELATIVE_PATH,
	PlexServerConnection = PLEX_SERVER_CONNECTION_RELATIVE_PATH,
	PlexLibrary = PLEX_LIBRARY_RELATIVE_PATH,
	PlexMedia = PLEX_MEDIA_RELATIVE_PATH,
	PlexAccount = PLEX_ACCOUNT_RELATIVE_PATH,
	Download = DOWNLOAD_RELATIVE_PATH,
	Settings = SETTINGS_RELATIVE_PATH,
	FolderPath = FOLDER_PATH_RELATIVE_PATH,
	Notification = NOTIFICATION_RELATIVE_PATH,
	ProgressHub = PROGRESS_HUB_RELATIVE_PATH,
	NotificationsHub = NOTIFICATIONS_RELATIVE_PATH,
}
