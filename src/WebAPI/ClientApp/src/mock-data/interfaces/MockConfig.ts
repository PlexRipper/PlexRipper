import { PlexMediaType } from '@dto/mainApi';

export interface MockConfig {
	seed: number;
	plexServerCount: number;
	plexLibraryCount: number;
	plexLibraryTypes: PlexMediaType[];
	plexAccountCount: number;
	plexServerAccessCount: number;
	firstTimeSetup: boolean;
	movieDownloadTask: number;
	tvShowDownloadTask: number;
	// region DownloadTasks
	seasonDownloadTask: number;
	// endregion
	episodeDownloadTask: number;
	// region DownloadTasks
	connectionHasProgress: boolean;
	// endregion
	maxServerConnections: number;
}
