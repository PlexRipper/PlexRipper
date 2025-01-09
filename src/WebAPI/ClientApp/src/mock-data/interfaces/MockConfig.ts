import type {
	FolderPathDTO,
	PlexAccountDTO,
	PlexLibraryDTO,
	PlexMediaType,
	PlexServerConnectionDTO,
	PlexServerDTO,
	ServerDownloadProgressDTO,
	SettingsModelDTO,
} from '@dto';

export interface MockConfig {
	seed: number;
	debugDisplayData: boolean;
	pageLoadDelay: number;
	isLoggedIn: boolean;
	// region Server
	/*
   * The number of Plex servers that are created
   * @default 5
   */
	plexServerCount: number;
	plexServerAccessCount: number;
	plexServerStatusCount: number;
	connectionHasProgress: boolean;
	// endregion
	plexMovieLibraryCount: number;
	plexTvShowLibraryCount: number;
	plexLibraryTypes: PlexMediaType[];
	/*
   * The number of Plex accounts that are created
   * @default 1
   */
	plexAccountCount: number;
	firstTimeSetup: boolean;
	// region DownloadTasks
	movieDownloadTask: number;
	tvShowDownloadTask: number;
	seasonDownloadTask: number;
	episodeDownloadTask: number;
	setDownloadDetails: boolean;
	// endregion
	// region PlexMedia
	movieCount: number;
	tvShowCount: number;
	seasonCount: number;
	episodeCount: number;
	// endregion

	maxServerConnections: number;
	folderPathCount: number;
	invalidDefaultFolderPaths: boolean;
	override: Partial<{
		folderPaths: (folderPaths: FolderPathDTO[]) => FolderPathDTO[];
		plexServer: (plexServers: PlexServerDTO[]) => PlexServerDTO[];
		plexServerConnections: (plexServerConnections: PlexServerConnectionDTO[]) => PlexServerConnectionDTO[];
		plexLibraries: (plexLibraries: PlexLibraryDTO[]) => PlexLibraryDTO[];
		plexAccounts: (plexAccounts: PlexAccountDTO[]) => PlexAccountDTO[];
		downloadTasks: (downloadTasks: ServerDownloadProgressDTO[]) => ServerDownloadProgressDTO[];
		settings: (settings: SettingsModelDTO) => SettingsModelDTO;
	}>;
}
