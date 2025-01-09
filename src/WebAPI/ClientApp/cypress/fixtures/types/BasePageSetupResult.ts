import type { MockConfig } from '@mock';
import {
	type IBasePageSetupResult,
	setupMockAuthenticationEndpoints,
	setupMockDownloadTasksEndpoints,
	setupMockFolderPathsEndpoints,
	setupMockNotificationsEndpoints,
	setupMockPlexAccountsEndpoints,
	setupMockPlexLibrariesEndpoints,
	setupMockPlexMediaEndpoints,
	setupMockPlexServerConnectionsEndpoints,
	setupMockPlexServersEndpoints,
	setupMockSettingsEndpoints,
	setupMockSignalREndpoints,
} from '@fixtures';
import type {
	DownloadTaskDTO,
	FolderPathDTO,
	PlexAccountDTO,
	PlexLibraryDTO, PlexMediaSlimDTO,
	PlexServerConnectionDTO,
	PlexServerDTO, ServerDownloadProgressDTO,
	SettingsModelDTO,
} from '@dto';

export class BasePageSetupResult implements IBasePageSetupResult {
	public plexServers: PlexServerDTO[] = [];
	public plexServerConnections: PlexServerConnectionDTO[] = [];
	public plexLibraries: PlexLibraryDTO[] = [];
	public plexAccounts: PlexAccountDTO[] = [];
	public serverDownloadProgress: ServerDownloadProgressDTO[] = [];
	public detailDownloadTasks: DownloadTaskDTO[] = [];
	public mediaData: {
		libraryId: number;
		media: PlexMediaSlimDTO[];
	}[] = [];

	public folderPaths: FolderPathDTO[] = [];
	public settings = {} as SettingsModelDTO;
	public config = {} as MockConfig;

	setupAuthenticationEndpoints(config: MockConfig) {
		return setupMockAuthenticationEndpoints.call(this, config);
	}

	setupPlexServersEndpoints(config: MockConfig) {
		return setupMockPlexServersEndpoints.call(this, config);
	}

	setupPlexServerConnectionsEndpoints(config: MockConfig) {
		return setupMockPlexServerConnectionsEndpoints.call(this, config);
	}

	setupPlexLibrariesEndpoints(config: MockConfig) {
		return setupMockPlexLibrariesEndpoints.call(this, config);
	}

	setupPlexAccountsEndpoints(config: MockConfig) {
		return setupMockPlexAccountsEndpoints.call(this, config);
	}

	setupDownloadTasksEndpoints(config: MockConfig) {
		return setupMockDownloadTasksEndpoints.call(this, config);
	}

	setupSettingsEndpoints(config: MockConfig) {
		return setupMockSettingsEndpoints.call(this, config);
	}

	setupPlexMediaEndpoints(config: MockConfig) {
		return setupMockPlexMediaEndpoints.call(this, config);
	}

	setupFolderPathsEndpoints(config: MockConfig) {
		return setupMockFolderPathsEndpoints.call(this, config);
	}

	setupNotificationsEndpoints() {
		return setupMockNotificationsEndpoints.call(this);
	}

	setupSignalREndpoints() {
		return setupMockSignalREndpoints.call(this);
	}
}
