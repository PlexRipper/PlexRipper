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

export interface ResultDTOOfFolderPathDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: FolderPathDTO | null;
}

export interface ReasonDTO {
	message?: string;
	metadata?: Record<string, any>;
}

export interface ErrorDTO {
	reasons?: ErrorDTO[];
	message?: string;
	metadata?: Record<string, any>;
}

export interface SuccessDTO {
	message?: string;
	metadata?: Record<string, any>;
}

export interface FolderPathDTO {
	/**
	 * @format int32
	 * @min 0
	 * @exclusiveMin true
	 */
	id?: number;
	folderType?: FolderType;
	mediaType?: PlexMediaType;
	/** @minLength 1 */
	displayName: string;
	/** @minLength 1 */
	directory: string;
	isValid?: boolean;
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
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
}

export interface CreateFolderPathEndpointRequest {
	folderPathDto: FolderPathDTO;
}

export interface ResultDTOOfListOfFolderPathDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: FolderPathDTO[] | null;
}

export interface ResultDTOOfFileSystemDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: FileSystemDTO | null;
}

export interface FileSystemDTO {
	parent?: string;
	directories?: FileSystemModelDTO[];
	files?: FileSystemModelDTO[];
}

export interface FileSystemModelDTO {
	type?: FileSystemEntityType;
	name?: string;
	path?: string;
	extension?: string;
	/** @format int64 */
	size?: number;
	/** @format date-time */
	lastModified?: string | null;
}

export enum FileSystemEntityType {
	Parent = 'Parent',
	Drive = 'Drive',
	Folder = 'Folder',
	File = 'File',
}

export interface UpdateFolderPathEndpointRequest {
	folderPathDto: FolderPathDTO;
}

export interface ResultDTOOfInt32 {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	/** @format int32 */
	value?: number;
}

export interface ResultDTOOfListOfNotificationDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: NotificationDTO[] | null;
}

export interface NotificationDTO {
	/** @format int32 */
	id?: number;
	level?: NotificationLevel;
	/** @format date-time */
	createdAt?: string;
	message?: string;
	hidden?: boolean;
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

export interface ErrorResponse {
	/**
	 * @format int32
	 * @default 400
	 */
	statusCode?: number;
	/** @default "One or more errors occurred!" */
	message?: string;
	errors?: Record<string, string[]>;
}

export interface ResultDTOOfListOfPlexServerStatusDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: PlexServerStatusDTO[] | null;
}

export interface PlexServerStatusDTO {
	/** @format int32 */
	id?: number;
	/** @format int32 */
	statusCode?: number;
	isSuccessful?: boolean;
	statusMessage?: string;
	/** @format date-time */
	lastChecked?: string;
	/** @format int32 */
	plexServerId?: number;
	/** @format int32 */
	plexServerConnectionId?: number;
}

export interface ResultDTOOfPlexServerStatusDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: PlexServerStatusDTO | null;
}

export interface ResultDTOOfPlexServerConnectionDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: PlexServerConnectionDTO | null;
}

export interface PlexServerConnectionDTO {
	/** @format int32 */
	id?: number;
	protocol?: string;
	address?: string;
	/** @format int32 */
	port?: number;
	local?: boolean;
	relay?: boolean;
	iPv4?: boolean;
	iPv6?: boolean;
	portFix?: boolean;
	/** @format int32 */
	plexServerId?: number;
	url?: string;
	serverStatusList?: PlexServerStatusDTO[];
	latestConnectionStatus?: PlexServerStatusDTO | null;
	progress?: ServerConnectionCheckStatusProgressDTO;
}

export interface ServerConnectionCheckStatusProgressDTO {
	/** @format int32 */
	plexServerId?: number;
	/** @format int32 */
	plexServerConnectionId?: number;
	/** @format int32 */
	retryAttemptIndex?: number;
	/** @format int32 */
	retryAttemptCount?: number;
	/** @format int32 */
	timeToNextRetry?: number;
	/** @format int32 */
	statusCode?: number;
	connectionSuccessful?: boolean;
	completed?: boolean;
	message?: string;
}

export interface ResultDTOOfListOfPlexServerConnectionDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: PlexServerConnectionDTO[] | null;
}

export interface ResultDTOOfPlexServerDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: PlexServerDTO | null;
}

export interface PlexServerDTO {
	/** @format int32 */
	id?: number;
	name?: string;
	/** @format int32 */
	ownerId?: number;
	plexServerOwnerUsername?: string;
	device?: string;
	platform?: string;
	platformVersion?: string;
	product?: string;
	productVersion?: string;
	provides?: string;
	/** @format date-time */
	createdAt?: string;
	/** @format date-time */
	lastSeenAt?: string;
	machineIdentifier?: string;
	publicAddress?: string;
	/** @format int32 */
	preferredConnectionId?: number;
	owned?: boolean;
	home?: boolean;
	synced?: boolean;
	relay?: boolean;
	presence?: boolean;
	httpsRequired?: boolean;
	publicAddressMatches?: boolean;
	dnsRebindingProtection?: boolean;
	natLoopbackSupported?: boolean;
	plexServerConnections?: PlexServerConnectionDTO[];
}

export interface ResultDTOOfListOfPlexServerDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: PlexServerDTO[] | null;
}

export interface ResultDTOOfSettingsModelDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: SettingsModelDTO | null;
}

export interface SettingsModelDTO {
	generalSettings: GeneralSettingsDTO;
	debugSettings: DebugSettingsDTO;
	confirmationSettings: ConfirmationSettingsDTO;
	dateTimeSettings: DateTimeSettingsDTO;
	displaySettings: DisplaySettingsDTO;
	downloadManagerSettings: DownloadManagerSettingsDTO;
	languageSettings: LanguageSettingsDTO;
	serverSettings: ServerSettingsDTO;
}

export interface GeneralSettingsDTO {
	firstTimeSetup?: boolean;
	/** @format int32 */
	activeAccountId?: number;
	debugMode?: boolean;
	disableAnimatedBackground?: boolean;
}

export interface DebugSettingsDTO {
	debugModeEnabled?: boolean;
	maskServerNames?: boolean;
	maskLibraryNames?: boolean;
}

export interface ConfirmationSettingsDTO {
	askDownloadMovieConfirmation?: boolean;
	askDownloadTvShowConfirmation?: boolean;
	askDownloadSeasonConfirmation?: boolean;
	askDownloadEpisodeConfirmation?: boolean;
}

export interface DateTimeSettingsDTO {
	shortDateFormat?: string;
	longDateFormat?: string;
	timeFormat?: string;
	timeZone?: string;
	showRelativeDates?: boolean;
}

export interface DisplaySettingsDTO {
	tvShowViewMode?: ViewMode;
	movieViewMode?: ViewMode;
}

export enum ViewMode {
	Table = 'Table',
	Poster = 'Poster',
}

export interface DownloadManagerSettingsDTO {
	/** @format int32 */
	downloadSegments?: number;
}

export interface LanguageSettingsDTO {
	language?: string;
}

export interface ServerSettingsDTO {
	data?: PlexServerSettingsModel[];
}

export interface PlexServerSettingsModel {
	plexServerName?: string | null;
	machineIdentifier?: string | null;
	/** @format int32 */
	downloadSpeedLimit?: number;
}

export interface UpdateUserSettingsEndpointRequest {
	settingsModelDto: SettingsModelDTO;
}

export interface DownloadMediaDTO {
	mediaIds?: number[];
	type?: PlexMediaType;
	/** @format int32 */
	plexServerId?: number;
	/** @format int32 */
	plexLibraryId?: number;
}

export interface ResultDTOOfDownloadTaskDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: DownloadTaskDTO | null;
}

export interface DownloadTaskDTO {
	/** @format int32 */
	id?: number;
	title?: string;
	fullTitle?: string;
	status?: DownloadStatus;
	fileLocationUrl?: string;
	downloadUrl?: string;
	fileName?: string;
	mediaType?: PlexMediaType;
	downloadTaskType?: DownloadTaskType;
	/** @format int32 */
	key?: number;
	/** @format int32 */
	downloadSpeed?: number;
	/** @format int64 */
	fileTransferSpeed?: number;
	/** @format int64 */
	dataReceived?: number;
	/** @format int64 */
	dataTotal?: number;
	/** @format decimal */
	percentage?: number;
	downloadDirectory?: string;
	destinationDirectory?: string;
	/** @format int32 */
	priority?: number;
	/** @format int32 */
	plexServerId?: number;
	/** @format int32 */
	plexLibraryId?: number;
	/** @format int32 */
	parentId?: number | null;
	/** @format int64 */
	timeRemaining?: number;
	quality?: string;
	children?: DownloadTaskDTO[];
	actions?: string[];
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

export interface ResultDTOOfListOfDownloadPreviewDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: DownloadPreviewDTO[] | null;
}

export interface DownloadPreviewDTO {
	/** @format int32 */
	id?: number;
	title?: string;
	/** @format int64 */
	size?: number;
	/** @format int32 */
	childCount?: number;
	mediaType?: PlexMediaType;
	children?: DownloadPreviewDTO[];
}

export interface ResultDTOOfListOfPlexAccountDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: PlexAccountDTO[] | null;
}

export interface PlexAccountDTO {
	/** @format int32 */
	id?: number;
	displayName?: string;
	username?: string;
	password?: string;
	isEnabled?: boolean;
	isMain?: boolean;
	isValidated?: boolean;
	/** @format date-time */
	validatedAt?: string;
	uuid?: string;
	/** @format int64 */
	plexId?: number;
	email?: string;
	title?: string;
	hasPassword?: boolean;
	authenticationToken?: string;
	clientId?: string;
	verificationCode?: string;
	is2Fa?: boolean;
	plexServerAccess?: PlexServerAccessDTO[];
}

export interface PlexServerAccessDTO {
	/** @format int32 */
	plexServerId?: number;
	plexLibraryIds?: number[];
}

export interface ResultDTOOfPlexAccountDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: PlexAccountDTO | null;
}

export interface ResultDTOOfBoolean {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: boolean;
}

export interface ResultDTOOfAuthPin {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: AuthPin | null;
}

export interface AuthPin {
	errors?: PlexError[];
	/** @format int32 */
	id?: number;
	code?: string;
	trusted?: boolean;
	clientIdentifier?: string;
	location?: AuthPinLocation;
	/** @format int32 */
	expiresIn?: number;
	/** @format date-time */
	createdAt?: string;
	/** @format date-time */
	expiresAt?: string;
	authToken?: string;
	newRegistration?: string;
}

export interface PlexError {
	message?: string | null;
	metadata?: Record<string, any>;
	reasons?: IError[] | null;
	/** @format int32 */
	code?: number;
	/** @format int32 */
	status?: number;
}

export interface IError {
	reasons?: IError[] | null;
}

export interface AuthPinLocation {
	code?: string;
	europeanUnionMember?: boolean;
	continentCode?: string;
	country?: string;
	city?: string;
	timeZone?: string;
	postalCode?: string;
	subdivisions?: string;
	coordinates?: string;
}

export interface ResultDTOOfListOfPlexLibraryDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: PlexLibraryDTO[] | null;
}

export interface PlexLibraryDTO {
	/** @format int32 */
	id?: number;
	key?: string;
	title?: string;
	type?: PlexMediaType;
	/** @format date-time */
	updatedAt?: string;
	/** @format date-time */
	createdAt?: string;
	/** @format date-time */
	scannedAt?: string;
	/** @format date-time */
	syncedAt?: string;
	outdated?: boolean;
	/** @format guid */
	uuid?: string;
	/** @format int64 */
	mediaSize?: number;
	/** @format int32 */
	libraryLocationId?: number;
	libraryLocationPath?: string;
	defaultDestination?: FolderPathDTO;
	/** @format int32 */
	defaultDestinationId?: number;
	/** @format int32 */
	plexServerId?: number;
	/** @format int32 */
	count?: number;
	/** @format int32 */
	seasonCount?: number;
	/** @format int32 */
	episodeCount?: number;
}

export interface ResultDTOOfPlexLibraryDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: PlexLibraryDTO | null;
}

export interface RefreshPlexLibraryDTO {
	/** @format int32 */
	plexAccountId?: number;
	/** @format int32 */
	plexLibraryId?: number;
}

export interface UpdateDefaultDestinationDTO {
	/** @format int32 */
	libraryId?: number;
	/** @format int32 */
	folderPathId?: number;
}

export interface ResultDTOOfPlexMediaSlimDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: PlexMediaSlimDTO | null;
}

export interface PlexMediaSlimDTO {
	/** @format int32 */
	id?: number;
	/** @format int32 */
	index?: number;
	title?: string;
	sortTitle?: string;
	/** @format int32 */
	year?: number;
	/** @format int32 */
	duration?: number;
	/** @format int64 */
	mediaSize?: number;
	/** @format int32 */
	childCount?: number;
	/** @format date-time */
	addedAt?: string;
	/** @format date-time */
	updatedAt?: string;
	/** @format int32 */
	plexLibraryId?: number;
	/** @format int32 */
	plexServerId?: number;
	type?: PlexMediaType;
	hasThumb?: boolean;
	thumbUrl?: string;
	qualities?: PlexMediaQuality[];
	children?: PlexMediaSlimDTO[];
}

export interface PlexMediaQuality {
	quality?: string | null;
	displayQuality?: string | null;
	hashId?: string | null;
}

export interface ResultDTOOfPlexMediaDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: PlexMediaDTO | null;
}

export interface PlexMediaDTO {
	/** @format int32 */
	id?: number;
	/** @format int32 */
	index?: number;
	title?: string;
	sortTitle?: string;
	/** @format int32 */
	year?: number;
	/** @format int32 */
	duration?: number;
	/** @format int64 */
	mediaSize?: number;
	/** @format int32 */
	childCount?: number;
	/** @format date-time */
	addedAt?: string;
	/** @format date-time */
	updatedAt?: string;
	/** @format int32 */
	plexLibraryId?: number;
	/** @format int32 */
	plexServerId?: number;
	type?: PlexMediaType;
	hasThumb?: boolean;
	thumbUrl?: string;
	qualities?: PlexMediaQuality[];
	children?: PlexMediaDTO[];
	/** @format int32 */
	key?: number;
	hasArt?: boolean;
	hasBanner?: boolean;
	hasTheme?: boolean;
	studio?: string;
	summary?: string;
	contentRating?: string;
	/** @format double */
	rating?: number;
	/** @format date-time */
	originallyAvailableAt?: string | null;
	/** @format int32 */
	tvShowId?: number;
	/** @format int32 */
	tvShowSeasonId?: number;
	mediaData?: PlexMediaDataDTO[];
}

export interface PlexMediaDataDTO {
	mediaFormat?: string;
	/** @format int64 */
	duration?: number;
	videoResolution?: string;
	/** @format int32 */
	width?: number;
	/** @format int32 */
	height?: number;
	/** @format int32 */
	bitrate?: number;
	videoCodec?: string;
	videoFrameRate?: string;
	/** @format double */
	aspectRatio?: number;
	videoProfile?: string;
	audioProfile?: string;
	audioCodec?: string;
	/** @format int32 */
	audioChannels?: number;
	parts?: PlexMediaDataPartDTO[];
}

export interface PlexMediaDataPartDTO {
	obfuscatedFilePath?: string;
	/** @format int32 */
	duration?: number;
	file?: string;
	/** @format int64 */
	size?: number;
	container?: string;
	videoProfile?: string;
}

export interface ResultDTOOfListOfPlexMediaSlimDTO {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[];
	errors?: ErrorDTO[];
	successes?: SuccessDTO[];
	value?: PlexMediaSlimDTO[] | null;
}
