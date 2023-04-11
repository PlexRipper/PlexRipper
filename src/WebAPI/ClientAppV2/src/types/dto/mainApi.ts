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

export interface AuthPinResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: AuthPin;
}

export interface BooleanResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: boolean;
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

export interface DownloadManagerSettingsDTO {
	/** @format int32 */
	downloadSegments: number;
}

export interface DownloadMediaDTO {
	mediaIds: number[];
	type: PlexMediaType;
	/** @format int32 */
	plexServerId: number;
	/** @format int32 */
	plexLibraryId: number;
}

export interface DownloadPreviewDTO {
	/** @format int32 */
	id: number;
	title: string;
	/** @format int64 */
	size: number;
	/** @format int32 */
	childCount: number;
	type: PlexMediaType;
	children: DownloadPreviewDTO[];
}

export interface DownloadPreviewDTOListResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: DownloadPreviewDTO[];
}

export interface DownloadProgressDTO {
	/** @format int32 */
	id: number;
	title: string;
	mediaType: PlexMediaType;
	status: string;
	/** @format double */
	percentage: number;
	/** @format int64 */
	dataReceived: number;
	/** @format int64 */
	dataTotal: number;
	/** @format int64 */
	downloadSpeed: number;
	/** @format int64 */
	fileTransferSpeed: number;
	/** @format int64 */
	timeRemaining: number;
	actions: string[];
	children: DownloadProgressDTO[];
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

export interface DownloadTaskDTO {
	/** @format int32 */
	id: number;
	title: string;
	fullTitle: string;
	status: DownloadStatus;
	fileLocationUrl: string;
	downloadUrl: string;
	fileName: string;
	mediaType: PlexMediaType;
	downloadTaskType: DownloadTaskType;
	/** @format int32 */
	key: number;
	/** @format int32 */
	downloadSpeed: number;
	/** @format int64 */
	fileTransferSpeed: number;
	/** @format int64 */
	dataReceived: number;
	/** @format int64 */
	dataTotal: number;
	/** @format double */
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
	quality: string;
	children: DownloadTaskDTO[];
	actions: string[];
}

export interface DownloadTaskDTOResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: DownloadTaskDTO;
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

export interface ErrorDTO {
	reasons: ErrorDTO[];
	message: string;
	metadata: Record<string, any>;
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
	/** @format double */
	percentage: number;
	/** @format int32 */
	transferSpeed: number;
	/** @format int64 */
	timeRemaining: number;
	/** @format int64 */
	bytesRemaining: number;
	/** @format int32 */
	plexServerId: number;
	/** @format int32 */
	plexLibraryId: number;
}

export interface FileSystemDTO {
	parent: string;
	directories: FileSystemModelDTO[];
	files: FileSystemModelDTO[];
}

export interface FileSystemDTOResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: FileSystemDTO;
}

export enum FileSystemEntityType {
	Parent = 'Parent',
	Drive = 'Drive',
	Folder = 'Folder',
	File = 'File',
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

export interface FolderPathDTO {
	/** @format int32 */
	id: number;
	folderType: FolderType;
	mediaType: PlexMediaType;
	displayName: string;
	directory: string;
	isValid: boolean;
}

export interface FolderPathDTOListResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: FolderPathDTO[];
}

export interface FolderPathDTOResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: FolderPathDTO;
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

export interface GeneralSettingsDTO {
	firstTimeSetup: boolean;
	/** @format int32 */
	activeAccountId: number;
}

export interface IError {
	reasons: IError[];
	message?: string | null;
	metadata?: Record<string, any>;
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

export interface Int32ResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	/** @format int32 */
	value: number;
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
	jobRuntime: TimeSpan;
	/** @format date-time */
	jobStartTime: string;
	status: JobStatus;
	primaryKey: string;
	/** @format int32 */
	primaryKeyValue: number;
}

export enum JobTypes {
	Unknown = 'Unknown',
	InspectPlexServerJob = 'InspectPlexServerJob',
	DownloadJob = 'DownloadJob',
	DownloadProgressJob = 'DownloadProgressJob',
	SyncServerJob = 'SyncServerJob',
	RefreshAccessiblePlexServersJob = 'RefreshAccessiblePlexServersJob',
	DownloadProgressJobs = 'DownloadProgressJobs',
	InspectPlexServerByPlexAccountIdJob = 'InspectPlexServerByPlexAccountIdJob',
}

export interface LanguageSettingsDTO {
	language: string;
}

export interface LibraryProgress {
	/** @format int32 */
	id: number;
	/** @format double */
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

export interface NotificationDTO {
	/** @format int32 */
	id: number;
	level: NotificationLevel;
	/** @format date-time */
	createdAt: string;
	message: string;
	hidden: boolean;
}

export interface NotificationDTOListResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: NotificationDTO[];
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

export interface PlexAccountDTOListResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: PlexAccountDTO[];
}

export interface PlexAccountDTOResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: PlexAccountDTO;
}

export interface PlexError {
	message: string;
	metadata: Record<string, any>;
	reasons: IError[];
	/** @format int32 */
	code: number;
	/** @format int32 */
	status: number;
}

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
	/** @format uuid */
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

export interface PlexLibraryDTOListResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: PlexLibraryDTO[];
}

export interface PlexLibraryDTOResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: PlexLibraryDTO;
}

export interface PlexMediaDTO {
	/** @format int32 */
	id: number;
	title: string;
	sortTitle: string;
	/** @format int32 */
	year: number;
	/** @format int32 */
	duration: number;
	/** @format int64 */
	mediaSize: number;
	/** @format int32 */
	childCount: number;
	/** @format date-time */
	addedAt: string;
	/** @format date-time */
	updatedAt: string;
	/** @format int32 */
	plexLibraryId: number;
	/** @format int32 */
	plexServerId: number;
	type: PlexMediaType;
	/** @format int32 */
	key: number;
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
	/** @format date-time */
	originallyAvailableAt: string;
	/** @format int32 */
	tvShowId: number;
	/** @format int32 */
	tvShowSeasonId: number;
	mediaData: PlexMediaDataDTO[];
	children: PlexMediaDTO[];
}

export interface PlexMediaDTOResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: PlexMediaDTO;
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

export interface PlexMediaSlimDTO {
	/** @format int32 */
	id: number;
	title: string;
	sortTitle: string;
	/** @format int32 */
	year: number;
	/** @format int32 */
	duration: number;
	/** @format int64 */
	mediaSize: number;
	/** @format int32 */
	childCount: number;
	/** @format date-time */
	addedAt: string;
	/** @format date-time */
	updatedAt: string;
	/** @format int32 */
	plexLibraryId: number;
	/** @format int32 */
	plexServerId: number;
	type: PlexMediaType;
}

export interface PlexMediaSlimDTOListResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: PlexMediaSlimDTO[];
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

export interface PlexServerAccessDTO {
	/** @format int32 */
	plexServerId: number;
	plexLibraryIds: number[];
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
	serverStatusList: PlexServerStatusDTO[];
	latestConnectionStatus: PlexServerStatusDTO;
	progress: ServerConnectionCheckStatusProgressDTO;
}

export interface PlexServerConnectionDTOListResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: PlexServerConnectionDTO[];
}

export interface PlexServerConnectionDTOResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: PlexServerConnectionDTO;
}

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
}

export interface PlexServerDTOListResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: PlexServerDTO[];
}

export interface PlexServerDTOResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: PlexServerDTO;
}

export interface PlexServerSettingsModel {
	plexServerName: string;
	machineIdentifier: string;
	/** @format int32 */
	downloadSpeedLimit: number;
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
	/** @format int32 */
	plexServerConnectionId: number;
}

export interface PlexServerStatusDTOListResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: PlexServerStatusDTO[];
}

export interface PlexServerStatusDTOResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: PlexServerStatusDTO;
}

export interface ReasonDTO {
	message: string;
	metadata: Record<string, any>;
}

export interface RefreshPlexLibraryDTO {
	/** @format int32 */
	plexAccountId: number;
	/** @format int32 */
	plexLibraryId: number;
}

export interface ResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
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

export interface ServerDownloadProgressDTO {
	/** @format int32 */
	id: number;
	downloads: DownloadProgressDTO[];
}

export interface ServerDownloadProgressDTOListResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: ServerDownloadProgressDTO[];
}

export interface ServerSettingsDTO {
	data: PlexServerSettingsModel[];
}

export interface SettingsModelDTO {
	generalSettings: GeneralSettingsDTO;
	confirmationSettings: ConfirmationSettingsDTO;
	dateTimeSettings: DateTimeSettingsDTO;
	displaySettings: DisplaySettingsDTO;
	downloadManagerSettings: DownloadManagerSettingsDTO;
	languageSettings: LanguageSettingsDTO;
	serverSettings: ServerSettingsDTO;
}

export interface SettingsModelDTOResultDTO {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value: SettingsModelDTO;
}

export interface SuccessDTO {
	message: string;
	metadata: Record<string, any>;
}

export interface SyncServerProgress {
	/** @format int32 */
	id: number;
	/** @format double */
	percentage: number;
	libraryProgresses: LibraryProgress[];
}

export interface TimeSpan {
	/** @format int64 */
	ticks: number;
	/** @format int32 */
	days: number;
	/** @format int32 */
	hours: number;
	/** @format int32 */
	milliseconds: number;
	/** @format int32 */
	microseconds: number;
	/** @format int32 */
	nanoseconds: number;
	/** @format int32 */
	minutes: number;
	/** @format int32 */
	seconds: number;
	/** @format double */
	totalDays: number;
	/** @format double */
	totalHours: number;
	/** @format double */
	totalMilliseconds: number;
	/** @format double */
	totalMicroseconds: number;
	/** @format double */
	totalNanoseconds: number;
	/** @format double */
	totalMinutes: number;
	/** @format double */
	totalSeconds: number;
}

export interface UpdateDefaultDestinationDTO {
	/** @format int32 */
	libraryId: number;
	/** @format int32 */
	folderPathId: number;
}

export enum ViewMode {
	None = 'None',
	Table = 'Table',
	Poster = 'Poster',
	Overview = 'Overview',
}
