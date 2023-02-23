/* eslint-disable */
/* tslint:disable */
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

export type ResultDTOOfListOfServerDownloadProgressDTO = ResultDTO & {
	value: ServerDownloadProgressDTO[];
};

export interface ServerDownloadProgressDTO {
	/** @format int32 */
	id: number;
	downloads: DownloadProgressDTO[];
}

export interface DownloadProgressDTO {
	/** @format int32 */
	id: number;
	title: string;
	mediaType: PlexMediaType;
	status: string;
	/** @format decimal */
	percentage: number;
	/** @format int64 */
	dataReceived: number;
	/** @format int64 */
	dataTotal: number;
	/** @format int64 */
	downloadSpeed: number;
	/** @format int64 */
	timeRemaining: number;
	actions: string[];
	children: DownloadProgressDTO[];
}

export enum PlexMediaType {
	None = 'None',
	Movie = 'Movie',
	TvShow = 'TvShow',
	Season = 'Season',
	Episode = 'Episode',
	Music = 'Music',
	Album = 'Album',
	Song = 'Song',
	Photos = 'Photos',
	OtherVideos = 'OtherVideos',
	Games = 'Games',
	Unknown = 'Unknown',
}

export interface ResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
}

export interface ReasonDTO {
	message: string;
	metadata: Record<string, any>;
}

export interface ErrorDTO {
	reasons?: ErrorDTO[];
	message: string;
	metadata: Record<string, any>;
}

export interface SuccessDTO {
	message: string;
	metadata: Record<string, any>;
}

export interface DownloadMediaDTO {
	mediaIds: number[];
	type: PlexMediaType;
	/** @format int32 */
	plexAccountId: number;
}

export type ResultDTOOfListOfFolderPathDTO = ResultDTO & {
	value: FolderPathDTO[];
};

export interface FolderPathDTO {
	/** @format int32 */
	id: number;
	folderType: FolderType;
	mediaType: PlexMediaType;
	displayName: string;
	directory: string;
	isValid: boolean;
}

export enum FolderType {
	None = 'None',
	DownloadFolder = 'DownloadFolder',
	MovieFolder = 'MovieFolder',
	TvShowFolder = 'TvShowFolder',
	MusicFolder = 'MusicFolder',
	PhotosFolder = 'PhotosFolder',
	OtherVideosFolder = 'OtherVideosFolder',
	GamesVideosFolder = 'GamesVideosFolder',
	Unknown = 'Unknown',
}

export type ResultDTOOfFileSystemDTO = ResultDTO & {
	value: FileSystemDTO;
};

export interface FileSystemDTO {
	parent: string;
	directories: FileSystemModelDTO[];
	files: FileSystemModelDTO[];
}

export interface FileSystemModelDTO {
	type: FileSystemEntityType;
	name: string;
	path: string;
	extension: string;
	/** @format int64 */
	size: number;
	/** @format date-time */
	lastModified: string;
}

export enum FileSystemEntityType {
	Parent = 'Parent',
	Drive = 'Drive',
	Folder = 'Folder',
	File = 'File',
}

export type ResultDTOOfFolderPathDTO = ResultDTO & {
	value: FolderPathDTO;
};

export type ResultDTOOfListOfNotificationDTO = ResultDTO & {
	value: NotificationDTO[];
};

export interface NotificationDTO {
	/** @format int32 */
	id: number;
	level: NotificationLevel;
	/** @format date-time */
	createdAt: string;
	message: string;
	hidden: boolean;
}

export enum NotificationLevel {
	None = 'None',
	Verbose = 'Verbose',
	Debug = 'Debug',
	Information = 'Information',
	Success = 'Success',
	Warning = 'Warning',
	Error = 'Error',
	Fatal = 'Fatal',
}

export type ResultDTOOfInteger = ResultDTO & {
	/** @format int32 */
	value: number;
};

export type ResultDTOOfListOfPlexAccountDTO = ResultDTO & {
	value: PlexAccountDTO[];
};

export interface PlexAccountDTO {
	/** @format int32 */
	id: number;
	displayName: string;
	username: string;
	password: string;
	isEnabled: boolean;
	isMain: boolean;
	isValidated: boolean;
	/** @format date-time */
	validatedAt: string;
	uuid: string;
	/** @format int64 */
	plexId: number;
	email: string;
	title: string;
	hasPassword: boolean;
	authenticationToken: string;
	clientId: string;
	verificationCode: string;
	is2Fa: boolean;
	plexServerAccess: PlexServerAccessDTO[];
}

export interface PlexServerAccessDTO {
	/** @format int32 */
	plexServerId: number;
	plexLibraryIds: number[];
}

export type ResultDTOOfPlexAccountDTO = ResultDTO & {
	value: PlexAccountDTO;
};

export type ResultDTOOfBoolean = ResultDTO & {
	value: boolean;
};

export type ResultDTOOfAuthPin = ResultDTO & {
	value: AuthPin;
};

export interface AuthPin {
	errors: PlexError[];
	/** @format int32 */
	id: number;
	code: string;
	trusted: boolean;
	clientIdentifier: string;
	location: AuthPinLocation;
	/** @format int32 */
	expiresIn: number;
	/** @format date-time */
	createdAt: string;
	/** @format date-time */
	expiresAt: string;
	authToken: string;
	newRegistration: string;
}

export type PlexError = Error & {
	/** @format int32 */
	code: number;
	/** @format int32 */
	status: number;
};

export interface Error {
	message?: string;
	metadata?: Record<string, any>;
	reasons?: IError[];
}

export interface IError {
	reasons?: IError[];
}

export interface AuthPinLocation {
	code: string;
	europeanUnionMember: boolean;
	continentCode: string;
	country: string;
	city: string;
	timeZone: string;
	postalCode: string;
	subdivisions: string;
	coordinates: string;
}

export type ResultDTOOfListOfPlexLibraryDTO = ResultDTO & {
	value: PlexLibraryDTO[];
};

export interface PlexLibraryDTO {
	/** @format int32 */
	id: number;
	key: string;
	title: string;
	type: PlexMediaType;
	/** @format date-time */
	updatedAt: string;
	/** @format date-time */
	createdAt: string;
	/** @format date-time */
	scannedAt: string;
	/** @format date-time */
	syncedAt: string;
	outdated: boolean;
	/** @format guid */
	uuid: string;
	/** @format int64 */
	mediaSize: number;
	/** @format int32 */
	libraryLocationId: number;
	libraryLocationPath: string;
	defaultDestination: FolderPathDTO;
	/** @format int32 */
	defaultDestinationId: number;
	/** @format int32 */
	plexServerId: number;
	/** @format int32 */
	count: number;
	/** @format int32 */
	seasonCount: number;
	/** @format int32 */
	episodeCount: number;
	movies: PlexMediaDTO[];
	tvShows: PlexMediaDTO[];
	downloadTasks: DownloadTaskDTO[];
}

export interface PlexMediaDTO {
	/** @format int32 */
	id: number;
	/** @format int32 */
	key: number;
	treeKeyId: string;
	title: string;
	/** @format int32 */
	year: number;
	/** @format int32 */
	duration: number;
	/** @format int64 */
	mediaSize: number;
	hasThumb: boolean;
	hasArt: boolean;
	hasBanner: boolean;
	hasTheme: boolean;
	/** @format int32 */
	index: number;
	studio: string;
	summary: string;
	contentRating: string;
	/** @format double */
	rating: number;
	/** @format int32 */
	childCount: number;
	/** @format date-time */
	addedAt: string;
	/** @format date-time */
	updatedAt: string;
	/** @format date-time */
	originallyAvailableAt: string;
	/** @format int32 */
	tvShowId: number;
	/** @format int32 */
	tvShowSeasonId: number;
	/** @format int32 */
	plexLibraryId: number;
	/** @format int32 */
	plexServerId: number;
	type: PlexMediaType;
	mediaData: PlexMediaDataDTO[];
	children: PlexMediaDTO[];
}

export interface PlexMediaDataDTO {
	mediaFormat: string;
	/** @format int64 */
	duration: number;
	videoResolution: string;
	/** @format int32 */
	width: number;
	/** @format int32 */
	height: number;
	/** @format int32 */
	bitrate: number;
	videoCodec: string;
	videoFrameRate: string;
	/** @format double */
	aspectRatio: number;
	videoProfile: string;
	audioProfile: string;
	audioCodec: string;
	/** @format int32 */
	audioChannels: number;
	parts: PlexMediaDataPartDTO[];
}

export interface PlexMediaDataPartDTO {
	obfuscatedFilePath: string;
	/** @format int32 */
	duration: number;
	file: string;
	/** @format int64 */
	size: number;
	container: string;
	videoProfile: string;
}

export interface DownloadTaskDTO {
	/** @format int32 */
	id: number;
	title: string;
	fullTitle: string;
	status: DownloadStatus;
	fileLocationUrl: string;
	fileName: string;
	mediaType: PlexMediaType;
	downloadTaskType: DownloadTaskType;
	/** @format int32 */
	key: number;
	/** @format int32 */
	downloadSpeed: number;
	/** @format int64 */
	dataReceived: number;
	/** @format int64 */
	dataTotal: number;
	/** @format decimal */
	percentage: number;
	downloadDirectory: string;
	destinationDirectory: string;
	/** @format int32 */
	priority: number;
	/** @format int32 */
	plexServerId: number;
	/** @format int32 */
	plexLibraryId: number;
	/** @format int32 */
	parentId: number;
	/** @format int64 */
	timeRemaining: number;
	downloadUrl: string;
	quality: string;
	children?: DownloadTaskDTO[];
	actions: string[];
}

export enum DownloadStatus {
	Unknown = 'Unknown',
	Error = 'Error',
	Queued = 'Queued',
	Downloading = 'Downloading',
	DownloadFinished = 'DownloadFinished',
	Paused = 'Paused',
	Stopped = 'Stopped',
	Deleted = 'Deleted',
	Merging = 'Merging',
	Moving = 'Moving',
	Completed = 'Completed',
}

export enum DownloadTaskType {
	None = 'None',
	Movie = 'Movie',
	MovieData = 'MovieData',
	MoviePart = 'MoviePart',
	TvShow = 'TvShow',
	Season = 'Season',
	Episode = 'Episode',
	EpisodeData = 'EpisodeData',
	EpisodePart = 'EpisodePart',
}

export type ResultDTOOfPlexLibraryDTO = ResultDTO & {
	value: PlexLibraryDTO;
};

export type ResultDTOOfPlexServerDTO = ResultDTO & {
	value: PlexServerDTO;
};

export interface PlexServerDTO {
	/** @format int32 */
	id: number;
	name: string;
	/** @format int32 */
	ownerId: number;
	plexServerOwnerUsername: string;
	device: string;
	platform: string;
	platformVersion: string;
	product: string;
	productVersion: string;
	provides: string;
	/** @format date-time */
	createdAt: string;
	/** @format date-time */
	lastSeenAt: string;
	machineIdentifier: string;
	publicAddress: string;
	/** @format int32 */
	preferredConnectionId: number;
	owned: boolean;
	home: boolean;
	synced: boolean;
	relay: boolean;
	presence: boolean;
	httpsRequired: boolean;
	publicAddressMatches: boolean;
	dnsRebindingProtection: boolean;
	natLoopbackSupported: boolean;
	plexServerConnections: PlexServerConnectionDTO[];
	downloadTasks: DownloadProgressDTO[];
}

export interface PlexServerConnectionDTO {
	/** @format int32 */
	id: number;
	protocol: string;
	address: string;
	/** @format int32 */
	port: number;
	local: boolean;
	relay: boolean;
	iPv4: boolean;
	iPv6: boolean;
	portFix: boolean;
	/** @format int32 */
	plexServerId: number;
	url: string;
	latestConnectionStatus: PlexServerStatusDTO;
	progress: ServerConnectionCheckStatusProgressDTO;
}

export interface PlexServerStatusDTO {
	/** @format int32 */
	id: number;
	/** @format int32 */
	statusCode: number;
	isSuccessful: boolean;
	statusMessage: string;
	/** @format date-time */
	lastChecked: string;
	/** @format int32 */
	plexServerId: number;
}

export interface ServerConnectionCheckStatusProgressDTO {
	/** @format int32 */
	plexServerId: number;
	/** @format int32 */
	plexServerConnectionId: number;
	/** @format int32 */
	retryAttemptIndex: number;
	/** @format int32 */
	retryAttemptCount: number;
	/** @format int32 */
	timeToNextRetry: number;
	/** @format int32 */
	statusCode: number;
	connectionSuccessful: boolean;
	completed: boolean;
	message: string;
}

export interface RefreshPlexLibraryDTO {
	/** @format int32 */
	plexAccountId: number;
	/** @format int32 */
	plexLibraryId: number;
}

export interface UpdateDefaultDestinationDTO {
	/** @format int32 */
	libraryId: number;
	/** @format int32 */
	folderPathId: number;
}

export type ResultDTOOfPlexMediaDTO = ResultDTO & {
	value: PlexMediaDTO;
};

export type ResultDTOOfListOfPlexServerConnectionDTO = ResultDTO & {
	value: PlexServerConnectionDTO[];
};

export type ResultDTOOfPlexServerConnectionDTO = ResultDTO & {
	value: PlexServerConnectionDTO;
};

export type ResultDTOOfPlexServerStatusDTO = ResultDTO & {
	value: PlexServerStatusDTO;
};

export type ResultDTOOfListOfPlexServerDTO = ResultDTO & {
	value: PlexServerDTO[];
};

export type ResultDTOOfSettingsModelDTO = ResultDTO & {
	value: SettingsModelDTO;
};

export interface SettingsModelDTO {
	generalSettings: GeneralSettingsDTO;
	confirmationSettings: ConfirmationSettingsDTO;
	dateTimeSettings: DateTimeSettingsDTO;
	displaySettings: DisplaySettingsDTO;
	downloadManagerSettings: DownloadManagerSettingsDTO;
	languageSettings: LanguageSettingsDTO;
	serverSettings: ServerSettingsDTO;
}

export interface GeneralSettingsDTO {
	firstTimeSetup: boolean;
	/** @format int32 */
	activeAccountId: number;
	/** @format int32 */
	test?: number;
}

export interface ConfirmationSettingsDTO {
	askDownloadMovieConfirmation: boolean;
	askDownloadTvShowConfirmation: boolean;
	askDownloadSeasonConfirmation: boolean;
	askDownloadEpisodeConfirmation: boolean;
}

export interface DateTimeSettingsDTO {
	shortDateFormat: string;
	longDateFormat: string;
	timeFormat: string;
	timeZone: string;
	showRelativeDates: boolean;
}

export interface DisplaySettingsDTO {
	tvShowViewMode: ViewMode;
	movieViewMode: ViewMode;
}

export enum ViewMode {
	None = 'None',
	Table = 'Table',
	Poster = 'Poster',
	Overview = 'Overview',
}

export interface DownloadManagerSettingsDTO {
	/** @format int32 */
	downloadSegments: number;
}

export interface LanguageSettingsDTO {
	language: string;
}

export interface ServerSettingsDTO {
	data: PlexServerSettingsModel[];
}

export interface PlexServerSettingsModel {
	plexServerName?: string;
	machineIdentifier?: string;
	/** @format int32 */
	downloadSpeedLimit: number;
}

export type ResultDTOOfDownloadTaskDTO = ResultDTO & {
	value: DownloadTaskDTO;
};

export enum MessageTypes {
	LibraryProgress = 'LibraryProgress',
	DownloadTaskCreationProgress = 'DownloadTaskCreationProgress',
	DownloadTaskUpdate = 'DownloadTaskUpdate',
	ServerDownloadProgress = 'ServerDownloadProgress',
	InspectServerProgress = 'InspectServerProgress',
	ServerConnectionCheckStatusProgress = 'ServerConnectionCheckStatusProgress',
	FileMergeProgress = 'FileMergeProgress',
	SyncServerProgress = 'SyncServerProgress',
	Notification = 'Notification',
	JobStatusUpdate = 'JobStatusUpdate',
}

export enum JobTypes {
	InspectPlexServerByPlexAccountIdJob = 'InspectPlexServerByPlexAccountIdJob',
	InspectPlexServerJob = 'InspectPlexServerJob',
	DownloadJob = 'DownloadJob',
	DownloadProgressJob = 'DownloadProgressJob',
	SyncServerJob = 'SyncServerJob',
	RefreshAccessiblePlexServersJob = 'RefreshAccessiblePlexServersJob',
	DownloadProgressJobs = 'DownloadProgressJobs',
}

export enum JobStatus {
	Started = 'Started',
	Running = 'Running',
	Completed = 'Completed',
}

export interface JobStatusUpdateDTO {
	id: string;
	jobName: string;
	jobGroup: string;
	jobType: JobTypes;
	/** @format duration */
	jobRuntime: string;
	/** @format date-time */
	jobStartTime: string;
	status: JobStatus;
	primaryKey: string;
	/** @format int32 */
	primaryKeyValue: number;
}

export interface DownloadTaskCreationProgress {
	/** @format decimal */
	percentage: number;
	/** @format int32 */
	current: number;
	/** @format int32 */
	total: number;
	isComplete: boolean;
}

export interface LibraryProgress {
	/** @format int32 */
	id: number;
	/** @format decimal */
	percentage: number;
	/** @format int32 */
	received: number;
	/** @format int32 */
	total: number;
	/** @format date-time */
	timeStamp: string;
	isRefreshing: boolean;
	isComplete: boolean;
}

export interface InspectServerProgressDTO {
	/** @format int32 */
	plexServerId: number;
	/** @format int32 */
	retryAttemptIndex: number;
	/** @format int32 */
	retryAttemptCount: number;
	/** @format int32 */
	timeToNextRetry: number;
	/** @format int32 */
	statusCode: number;
	connectionSuccessful: boolean;
	completed: boolean;
	message: string;
	plexServerConnection: PlexServerConnectionDTO;
}

export interface FileMergeProgress {
	/** @format int32 */
	id: number;
	/** @format int32 */
	downloadTaskId: number;
	/** @format int64 */
	dataTransferred: number;
	/** @format int64 */
	dataTotal: number;
	/** @format decimal */
	percentage: number;
	/** @format int32 */
	transferSpeed: number;
	transferSpeedFormatted: string;
	/** @format int64 */
	timeRemaining: number;
	/** @format int64 */
	bytesRemaining: number;
	/** @format int32 */
	plexServerId: number;
	/** @format int32 */
	plexLibraryId: number;
}

export interface SyncServerProgress {
	/** @format int32 */
	id: number;
	/** @format decimal */
	percentage: number;
	libraryProgresses: LibraryProgress[];
}
