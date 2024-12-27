import type {
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
	plexServerCount: number;
	plexServerAccessCount: number;
	plexServerStatusCount: number;
	connectionHasProgress: boolean;
	// endregion
	plexMovieLibraryCount: number;
	plexTvShowLibraryCount: number;
	plexLibraryTypes: PlexMediaType[];
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
	override: Partial<{
		plexServer: (plexServers: PlexServerDTO[]) => PlexServerDTO[];
		plexServerConnections: (plexServerConnections: PlexServerConnectionDTO[]) => PlexServerConnectionDTO[];
		plexLibraries: (plexLibraries: PlexLibraryDTO[]) => PlexLibraryDTO[];
		plexAccounts: (plexAccounts: PlexAccountDTO[]) => PlexAccountDTO[];
		downloadTasks: (downloadTasks: ServerDownloadProgressDTO[]) => ServerDownloadProgressDTO[];
		settings: (settings: SettingsModelDTO) => SettingsModelDTO;
	}>;
}
