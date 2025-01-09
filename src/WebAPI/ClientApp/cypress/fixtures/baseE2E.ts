import { checkConfig, type MockConfig } from '@mock';
import { type IBasePageSetupResult, BasePageSetupResult } from '@fixtures';

export function basePageSetup(config: Partial<MockConfig> = {}): Cypress.Chainable<IBasePageSetupResult> {
	const validConfig = checkConfig(config);
	const result = new BasePageSetupResult();

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

	// Authentication call
	result.setupAuthenticationEndpoints(validConfig);

	// PlexServers call
	result.setupPlexServersEndpoints(validConfig);

	// PlexServerConnections call
	result.setupPlexServerConnectionsEndpoints(validConfig);

	// PlexLibraries call
	result.setupPlexLibrariesEndpoints(validConfig);

	// PlexAccount call
	result.setupPlexAccountsEndpoints(validConfig);

	// DownloadTasks call
	result.setupDownloadTasksEndpoints(validConfig);

	// Settings call
	result.setupSettingsEndpoints(validConfig);

	// PlexMedia call
	result.setupPlexMediaEndpoints(validConfig);

	// FolderPaths call
	result.setupFolderPathsEndpoints(validConfig);

	// SignalR call
	result.setupSignalREndpoints();

	// Notifications call
	result.setupNotificationsEndpoints();

	// Calculate library media size and count
	for (const library of result.plexLibraries) {
		const mediaList = result.mediaData.find((x) => x.libraryId === library.id)?.media ?? [];
		if (mediaList.length) {
			library.mediaSize = mediaList.reduce((acc, x) => acc + x.mediaSize, 0);
			library.count = mediaList.length;
		}
	}

	return cy.wrap(result as IBasePageSetupResult);
}

export function route(path: string) {
	return Cypress.env('BASE_URL') + path;
}
