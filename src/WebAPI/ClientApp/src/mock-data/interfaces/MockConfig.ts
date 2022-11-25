import { PlexMediaType } from '@dto/mainApi';

export interface MockConfig {
	seed?: number;
	plexServerCount?: number;
	plexLibraryCount?: number;
	plexLibraryTypes?: PlexMediaType[];
	plexAccountCount?: number;
	firstTimeSetup?: boolean;
	movieDownloadTask?: number;
	tvShowDownloadTask?: number;
	seasonDownloadTask?: number;
	episodeDownloadTask?: number;
}
