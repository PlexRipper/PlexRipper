export interface MockConfig {
	seed?: number;
	plexServerCount?: number;
	plexAccountCount?: number;
	firstTimeSetup?: boolean;
	movieDownloadTask?: number;
	tvShowDownloadTask?: number;
	seasonDownloadTask?: number;
	episodeDownloadTask?: number;
}
