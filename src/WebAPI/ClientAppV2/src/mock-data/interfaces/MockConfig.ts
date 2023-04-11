import { PlexMediaType } from '@dto/mainApi';

export interface MockConfig {
	seed: number;
	plexServerCount: number;
	plexLibraryCount: number;
	plexLibraryTypes: PlexMediaType[];
	plexAccountCount: number;
	plexServerAccessCount: number;
	firstTimeSetup: boolean;
	// region DownloadTasks
	movieDownloadTask: number;
	tvShowDownloadTask: number;
	seasonDownloadTask: number;
	episodeDownloadTask: number;
	// endregion
	// region Server
	connectionHasProgress: boolean;
	// endregion
	maxServerConnections: number;
}
