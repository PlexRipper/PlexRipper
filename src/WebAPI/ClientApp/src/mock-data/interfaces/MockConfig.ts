import type { PlexMediaType } from '@dto';

export interface MockConfig {
	seed: number;
	debugDisplayData: boolean;
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
}
