import {
	checkConfig,
	generateDownloadTask,
	generatePlexLibrariesFromPlexServers,
	generatePlexMedia,
	generatePlexMediaSlims,
	generatePlexServerConnections,
	generatePlexServers,
	generateResultDTO,
	generateServerDownloadProgress,
	type MockConfig,
} from '@mock';
import { generateSettingsModel } from '@factories/settings-factory';
import { generatePlexAccounts } from '@factories/plex-account-factory';
import {
	type DownloadTaskDTO,
	type PlexAccountDTO,
	type PlexLibraryDTO,
	type PlexMediaSlimDTO,
	PlexMediaType,
	type PlexServerConnectionDTO,
	type PlexServerDTO,
	type ServerDownloadProgressDTO,
	type SettingsModelDTO,
} from '@dto';
import {
	DownloadPaths,
	FolderPathPaths,
	NotificationPaths,
	PlexAccountPaths,
	PlexLibraryPaths,
	PlexMediaPaths,
	PlexServerConnectionPaths,
	PlexServerPaths,
	SettingsPaths,
} from '@api/api-paths';
import Convert from '@class/Convert';

const headers = {
	headers: {
		'x-plexripper-version': '1.0.0',
	},
};

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

export function basePageSetup(config: Partial<MockConfig> = {}): Cypress.Chainable<IBasePageSetupResult> {
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

	if (
		config.override === undefined
		|| !config.override.plexServer
		|| !config.override.plexServerConnections
		|| !config.override.plexLibraries
		|| !config.override.plexAccounts
		|| !config.override.downloadTasks
		|| !config.override.settings
	) {
		throw new Error('All override properties must be defined.');
	}

	// PlexServers call
	result.plexServers = config.override.plexServer(generatePlexServers({ config }));
	cy.intercept('GET', PlexServerPaths.getAllPlexServersEndpoint(), {
		statusCode: 200,
		body: generateResultDTO(result.plexServers),
		...headers,
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> plexServers', result.plexServers);
		}
	});

	// PlexServerConnections call
	for (const plexServer of result.plexServers) {
		result.plexServerConnections.push(...generatePlexServerConnections({ plexServerId: plexServer.id, config }));
	}
	result.plexServerConnections = config.override.plexServerConnections(result.plexServerConnections);
	cy.intercept('GET', PlexServerConnectionPaths.getAllPlexServerConnectionsEndpoint(), {
		statusCode: 200,
		body: generateResultDTO(result.plexServerConnections),
		...headers,
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> plexServerConnections', result.plexServerConnections);
		}
	});

	// PlexLibraries call
	result.plexLibraries = generatePlexLibrariesFromPlexServers({ plexServers: result.plexServers, config });
	result.plexLibraries = config.override.plexLibraries(result.plexLibraries);
	cy.intercept('GET', PlexLibraryPaths.getAllPlexLibrariesEndpoint(), {
		statusCode: 200,
		body: generateResultDTO(result.plexLibraries),
		...headers,
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> plexLibraries', result.plexLibraries);
		}
	});

	// Detail call for every library
	for (const library of result.plexLibraries) {
		cy.intercept('GET', PlexLibraryPaths.getPlexLibraryByIdEndpoint(library.id), {
			statusCode: 200,
			body: generateResultDTO(library),
			...headers,
		});
	}

	// PlexAccount call
	result.plexAccounts = generatePlexAccounts({
		config,
		plexServers: result.plexServers,
		plexLibraries: result.plexLibraries,
	});
	result.plexAccounts = config.override.plexAccounts(result.plexAccounts);
	cy.intercept('GET', PlexAccountPaths.getAllPlexAccountsEndpoint(), {
		statusCode: 200,
		body: generateResultDTO(result.plexAccounts),
		...headers,
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
	result.serverDownloadProgress = config.override.downloadTasks(result.serverDownloadProgress);
	cy.intercept('GET', DownloadPaths.getAllDownloadTasksEndpoint(), {
		statusCode: 200,
		body: generateResultDTO(result.serverDownloadProgress),
		...headers,
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
					type: Convert.toDownloadTaskType(downloadTask.mediaType),
					// partial: downloadTask,
				});
				result.detailDownloadTasks.push(generatedDownloadTask);
				cy.intercept('GET', DownloadPaths.getDownloadTaskByGuidEndpoint(downloadTask.id), {
					statusCode: 200,
					body: generateResultDTO(generatedDownloadTask),
					...headers,
				});
			}
		}
	}

	// Settings call
	result.settings = generateSettingsModel({ plexServers: result.plexServers, config });
	result.settings = config.override.settings(result.settings);
	cy.intercept('GET', SettingsPaths.getUserSettingsEndpoint(), {
		statusCode: 200,
		body: generateResultDTO(result.settings),
		...headers,
	}).then(() => {
		if (validConfig.debugDisplayData) {
			cy.log('BasePageSetup -> settings', result.settings);
		}
	});

	// Generate library media page data
	for (const library of result.plexLibraries) {
		const mediaList = generatePlexMediaSlims({
			config,
			partialData: {
				plexLibraryId: library.id,
				plexServerId: library.plexServerId,
				type: library.type,
			},
		});

		result.mediaData.push({
			libraryId: library.id,
			media: mediaList,
		});

		for (const mediaItem of mediaList) {
			cy.intercept(
				'GET',
				PlexLibraryPaths.getPlexLibraryMediaEndpoint(library.id, {
					page: 0,
					size: 0,
				}),
				{
					statusCode: 200,
					body: generateResultDTO(mediaList),
					...headers,

				},
			);

			if (mediaItem.type === PlexMediaType.TvShow) {
				cy.intercept(
					'GET',
					PlexMediaPaths.getMediaDetailByIdEndpoint(mediaItem.id, {
						type: library.type,
					}),
					{
						statusCode: 200,
						body: generateResultDTO(
							generatePlexMedia({
								config,
								partialData: {
									type: PlexMediaType.TvShow,
									id: mediaItem.id,
									plexLibraryId: library.id,
									plexServerId: library.plexServerId,
								},
							}),
						),
						...headers,
					},
				);
			}
		}
	}

	for (const mediaType of [PlexMediaType.Movie, PlexMediaType.TvShow]) {
		cy.intercept(
			'GET',
			PlexMediaPaths.getAllMediaByTypeEndpoint({
				mediaType,
				page: 0,
				size: 0,
				filterOwnedMedia: false,
				filterOfflineMedia: false,
			}),
			{
				statusCode: 200,
				body: generateResultDTO(
					result.mediaData.filter((x) => x.media.some((y) => y.type === mediaType)).flatMap((x) => x.media),
				),
				...headers,
			},
		);
	}

	cy.intercept('GET', FolderPathPaths.getAllFolderPathsEndpoint(), {
		statusCode: 200,
		body: generateResultDTO([]),
		...headers,
	});

	cy.intercept('GET', NotificationPaths.getAllNotificationsEndpoint(), {
		statusCode: 200,
		body: generateResultDTO([]),
		...headers,
	});

	cy.intercept('GET', '/progress', {
		statusCode: 200,
		body: {},
		...headers,
	});

	cy.intercept('GET', '/notifications', {
		statusCode: 200,
		body: {},
		...headers,
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
