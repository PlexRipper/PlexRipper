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
	generateResultDTO,
	MockConfig,
	checkConfig,
	generateServerDownloadProgress,
	generatePlexMedias,
	generateDownloadTask,
	generatePlexLibrariesFromPlexServers,
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
	DownloadTaskDTO,
} from '@dto/mainApi';
import Chainable = Cypress.Chainable;
import { toDownloadTaskType } from '@composables/conversion';

export interface IBasePageSetupResult {
	plexServers: PlexServerDTO[];
	plexServerConnections: PlexServerConnectionDTO[];
	plexLibraries: PlexLibraryDTO[];
	plexAccounts: PlexAccountDTO[];
	serverDownloadProgress: ServerDownloadProgressDTO[];
	detailDownloadTasks: DownloadTaskDTO[];
	mediaData: {
		libraryId: number;
		media: PlexMediaSlimDTO[];
	}[];
	settings: SettingsModelDTO;
	config: MockConfig;
}

export function basePageSetup(config: Partial<MockConfig> = {}): Chainable<IBasePageSetupResult> {
	const validConfig = checkConfig(config);
	const result: IBasePageSetupResult = {
		config: validConfig,
		mediaData: [],
		plexAccounts: [],
		plexLibraries: [],
		plexServerConnections: [],
		serverDownloadProgress: [],
		settings: {} as SettingsModelDTO,
		plexServers: [],
		detailDownloadTasks: [],
	};

	// PlexServers call
	result.plexServers = generatePlexServers({ config });
	cy.intercept('GET', apiRoute({ type: APIRoute.PlexServer }), {
		statusCode: 200,
		body: generateResultDTO(result.plexServers),
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> plexServers', result.plexServers);
		}
	});

	// PlexServerConnections call
	result.plexServerConnections = result.plexServers.flatMap((x) => x.plexServerConnections);
	cy.intercept('GET', apiRoute({ type: APIRoute.PlexServerConnection }), {
		statusCode: 200,
		body: generateResultDTO(result.plexServerConnections),
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> plexServerConnections', result.plexServerConnections);
		}
	});

	// PlexLibraries call
	result.plexLibraries = generatePlexLibrariesFromPlexServers({ plexServers: result.plexServers, config });

	cy.intercept('GET', apiRoute({ type: APIRoute.PlexLibrary }), {
		statusCode: 200,
		body: generateResultDTO(result.plexLibraries),
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> plexLibraries', result.plexLibraries);
		}
	});

	// Detail call for every library
	for (const library of result.plexLibraries) {
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
	result.plexAccounts = generatePlexAccounts({ config, plexServers: result.plexServers, plexLibraries: result.plexLibraries });
	cy.intercept('GET', apiRoute({ type: APIRoute.PlexAccount }), {
		statusCode: 200,
		body: generateResultDTO(result.plexAccounts),
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> plexAccounts', result.plexAccounts);
		}
	});

	// DownloadTasks call
	result.serverDownloadProgress = result.plexServers
		.map((x) =>
			generateServerDownloadProgress({
				plexServerId: x.id,
				plexLibraryId: -1,
				config,
			}),
		)
		.flat();
	cy.intercept('GET', apiRoute({ type: APIRoute.Download }), {
		statusCode: 200,
		body: generateResultDTO(result.serverDownloadProgress),
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> downloadTasks', result.serverDownloadProgress);
		}
	});

	// DownloadDetails call
	if (config.setDownloadDetails) {
		for (const serverDownload of result.serverDownloadProgress) {
			const downloadTasks = result.serverDownloadProgress.flatMap((x) => x.downloads);
			for (const downloadTask of downloadTasks) {
				const generatedDownloadTask = generateDownloadTask({
					config,
					id: downloadTask.id,
					plexLibraryId: 1,
					plexServerId: serverDownload.id,
					type: toDownloadTaskType(downloadTask.mediaType),
					// partial: downloadTask,
				});
				result.detailDownloadTasks.push(generatedDownloadTask);
				cy.intercept(
					'GET',
					apiRoute({
						type: APIRoute.Download,
						path: `/detail/${downloadTask.id}`,
					}),
					{
						statusCode: 200,
						body: generateResultDTO(generatedDownloadTask),
					},
				);
			}
		}
	}

	// Settings call
	result.settings = generateSettingsModel({ plexServers: result.plexServers, config });
	cy.intercept('GET', apiRoute({ type: APIRoute.Settings }), {
		statusCode: 200,
		body: generateResultDTO(result.settings),
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> settings', result.settings);
		}
	});

	// Generate library media page data
	for (const library of result.plexLibraries) {
		if (library.type !== PlexMediaType.Movie) {
			continue;
		}
		const mediaList = generatePlexMedias({
			plexLibraryId: library.id,
			plexServerId: library.plexServerId,
			type: library.type,
			config,
		});

		result.mediaData.push({
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
	for (const library of result.plexLibraries) {
		const mediaList = result.mediaData.find((x) => x.libraryId === library.id)?.media ?? [];
		if (mediaList.length) {
			library.mediaSize = mediaList.reduce((acc, x) => acc + x.mediaSize, 0);
			library.count = mediaList.length;
		}
	}

	return cy.wrap(result);
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
