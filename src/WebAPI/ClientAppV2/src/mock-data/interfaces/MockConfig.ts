import { PlexMediaType } from '@dto/mainApi';

export interface MockConfig {
	seed: number;
	debugDisplayData: boolean;
	// region Server
	plexServerCount: number;
	plexServerAccessCount: number;
	plexServerStatusCount: number;
	connectionHasProgress: boolean;
	// endregion
	plexLibraryCount: number;
	plexLibraryTypes: PlexMediaType[];
	plexAccountCount: number;
	firstTimeSetup: boolean;
	// region DownloadTasks
	movieDownloadTask: number;
	tvShowDownloadTask: number;
	seasonDownloadTask: number;
	episodeDownloadTask: number;
	// endregion
	maxServerConnections: number;
}
