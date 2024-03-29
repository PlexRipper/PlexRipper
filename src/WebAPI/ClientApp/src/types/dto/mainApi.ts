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
	authToken: string;
	clientIdentifier: string;
	code: string;
	/** @format date-time */
	createdAt: string;
	errors: PlexError[];
	/** @format date-time */
	expiresAt: string;
	/** @format int32 */
	expiresIn: number;
	/** @format int32 */
	id: number;
	location: AuthPinLocation;
	newRegistration: string;
	trusted: boolean;
}

export interface AuthPinLocation {
	city: string;
	code: string;
	continentCode: string;
	coordinates: string;
	country: string;
	europeanUnionMember: boolean;
	postalCode: string;
	subdivisions: string;
	timeZone: string;
}

export interface ConfirmationSettingsDTO {
	askDownloadEpisodeConfirmation: boolean;
	askDownloadMovieConfirmation: boolean;
	askDownloadSeasonConfirmation: boolean;
	askDownloadTvShowConfirmation: boolean;
}

export interface CreateDownloadTasksEndpointRequest {
	/** @minLength 1 */
	downloadMedias: DownloadMediaDTO[];
}

export interface CreateFolderPathEndpointRequest {
	folderPathDto: FolderPathDTO;
}

export interface CreatePlexAccountEndpointRequest {
	plexAccount?: PlexAccountDTO | null;
}

export interface DateTimeSettingsDTO {
	longDateFormat: string;
	shortDateFormat: string;
	showRelativeDates: boolean;
	timeFormat: string;
	timeZone: string;
}

export interface DebugSettingsDTO {
	debugModeEnabled: boolean;
	maskLibraryNames: boolean;
	maskServerNames: boolean;
}

export interface DeleteDownloadTaskEndpointRequest {
	/** @minLength 1 */
	downloadTaskIds: string[];
}

export interface DisplaySettingsDTO {
	movieViewMode: ViewMode;
	tvShowViewMode: ViewMode;
}

export interface DownloadManagerSettingsDTO {
	/** @format int32 */
	downloadSegments: number;
}

export interface DownloadMediaDTO {
	mediaIds: number[];
	/** @format int32 */
	plexLibraryId: number;
	/** @format int32 */
	plexServerId: number;
	type: PlexMediaType;
}

export interface DownloadPreviewDTO {
	/** @format int32 */
	childCount: number;
	children: DownloadPreviewDTO[];
	/** @format int32 */
	id: number;
	mediaType: PlexMediaType;
	/** @format int64 */
	size: number;
	title: string;
}

export interface DownloadProgressDTO {
	actions: string[];
	children: DownloadProgressDTO[];
	/** @format int64 */
	dataReceived: number;
	/** @format int64 */
	dataTotal: number;
	/** @format int64 */
	downloadSpeed: number;
	/** @format int64 */
	fileTransferSpeed: number;
	/** @format guid */
	id: string;
	mediaType: PlexMediaType;
	/** @format decimal */
	percentage: number;
	status: DownloadStatus;
	/** @format int64 */
	timeRemaining: number;
	title: string;
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

export interface DownloadTaskCreationProgress {
	/** @format int32 */
	current: number;
	/** Has the library finished refreshing. */
	isComplete: boolean;
	/** @format decimal */
	percentage: number;
	/** @format int32 */
	total: number;
}

export interface DownloadTaskDTO {
	actions: string[];
	children: DownloadTaskDTO[];
	/** @format date-time */
	createdAt: string;
	/** @format int64 */
	dataReceived: number;
	/** @format int64 */
	dataTotal: number;
	destinationDirectory: string;
	downloadDirectory: string;
	/** @format int64 */
	downloadSpeed: number;
	downloadTaskType: DownloadTaskType;
	downloadUrl: string;
	fileLocationUrl: string;
	fileName: string;
	/** @format int64 */
	fileTransferSpeed: number;
	fullTitle: string;
	/** @format guid */
	id: string;
	/** @format int32 */
	key: number;
	mediaType: PlexMediaType;
	/** @format guid */
	parentId: string;
	/** @format decimal */
	percentage: number;
	/** @format int32 */
	plexLibraryId: number;
	/** @format int32 */
	plexServerId: number;
	status: DownloadStatus;
	/** @format int64 */
	timeRemaining: number;
	title: string;
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
	message: string;
	metadata: Record<string, any>;
	reasons: IError[];
}

export interface ErrorResponse {
	errors: Record<string, string[]>;
	/** @default "One or more errors occurred!" */
	message: string;
	/**
	 * @format int32
	 * @default 400
	 */
	statusCode: number;
}

export interface FileMergeProgress {
	/** @format int64 */
	bytesRemaining: number;
	/** @format int64 */
	dataTotal: number;
	/** @format int64 */
	dataTransferred: number;
	/** @format guid */
	downloadTaskId: string;
	downloadTaskType: DownloadTaskType;
	/** @format int32 */
	id: number;
	/** @format decimal */
	percentage: number;
	/** @format int32 */
	plexLibraryId: number;
	/** @format int32 */
	plexServerId: number;
	/** @format int64 */
	timeRemaining: number;
	/** @format int32 */
	transferSpeed: number;
}

export interface FileSystemDTO {
	directories: FileSystemModelDTO[];
	files: FileSystemModelDTO[];
	parent: string;
}

export enum FileSystemEntityType {
	Parent = 'Parent',
	Drive = 'Drive',
	Folder = 'Folder',
	File = 'File',
}

export interface FileSystemModelDTO {
	extension: string;
	/** @format date-time */
	lastModified?: string | null;
	name: string;
	path: string;
	/** @format int64 */
	size: number;
	type: FileSystemEntityType;
}

export interface FolderPathDTO {
	/** @minLength 1 */
	directory: string;
	/** @minLength 1 */
	displayName: string;
	folderType: FolderType;
	/**
	 * @format int32
	 * @min 0
	 * @exclusiveMin true
	 */
	id: number;
	isValid: boolean;
	mediaType: PlexMediaType;
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
	/** @format int32 */
	activeAccountId: number;
	debugMode: boolean;
	disableAnimatedBackground: boolean;
	firstTimeSetup: boolean;
}

export interface IError {
	reasons?: IError[] | null;
}

export interface InspectServerProgressDTO {
	completed: boolean;
	connectionSuccessful: boolean;
	message: string;
	plexServerConnection: PlexServerConnectionDTO;
	/** @format int32 */
	plexServerId: number;
	/** @format int32 */
	retryAttemptCount: number;
	/** @format int32 */
	retryAttemptIndex: number;
	/** @format int32 */
	statusCode: number;
	/** @format int32 */
	timeToNextRetry: number;
}

export enum JobStatus {
	Started = 'Started',
	Running = 'Running',
	Completed = 'Completed',
}

export interface JobStatusUpdateDTO {
	id: string;
	jobGroup: string;
	jobName: string;
	/** @format duration */
	jobRuntime: string;
	/** @format date-time */
	jobStartTime: string;
	jobType: JobTypes;
	primaryKey: string;
	primaryKeyValue: string;
	status: JobStatus;
}

export enum JobTypes {
	Unknown = 'Unknown',
	InspectPlexServerJob = 'InspectPlexServerJob',
	DownloadJob = 'DownloadJob',
	DownloadProgressJob = 'DownloadProgressJob',
	SyncServerJob = 'SyncServerJob',
	RefreshPlexServersAccessJob = 'RefreshPlexServersAccessJob',
	DownloadProgressJobs = 'DownloadProgressJobs',
	InspectPlexServerByPlexAccountIdJob = 'InspectPlexServerByPlexAccountIdJob',
}

export interface LanguageSettingsDTO {
	language: string;
}

export interface LibraryProgress {
	/** @format int32 */
	id: number;
	isComplete: boolean;
	isRefreshing: boolean;
	/** @format decimal */
	percentage: number;
	/** @format int32 */
	received: number;
	/** @format date-time */
	timeStamp: string;
	/** @format int32 */
	total: number;
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
	/** @format date-time */
	createdAt: string;
	hidden: boolean;
	/** @format int32 */
	id: number;
	level: NotificationLevel;
	message: string;
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
	is2Fa: boolean;
	authenticationToken: string;
	clientId: string;
	/** @minLength 1 */
	displayName: string;
	email: string;
	hasPassword: boolean;
	/** @format int32 */
	id: number;
	isEnabled: boolean;
	isMain: boolean;
	isValidated: boolean;
	/** @minLength 1 */
	password: string;
	/** @format int64 */
	plexId: number;
	plexServerAccess: PlexServerAccessDTO[];
	title: string;
	/** @minLength 1 */
	username: string;
	uuid: string;
	/** @format date-time */
	validatedAt: string;
	verificationCode: string;
}

export interface PlexError {
	/** @format int32 */
	code: number;
	message?: string | null;
	metadata?: Record<string, any>;
	reasons?: IError[] | null;
	/** @format int32 */
	status: number;
}

export interface PlexLibraryDTO {
	/** @format int32 */
	count: number;
	/** @format date-time */
	createdAt: string;
	defaultDestination: FolderPathDTO;
	/** @format int32 */
	defaultDestinationId: number;
	/** @format int32 */
	episodeCount: number;
	/** @format int32 */
	id: number;
	key: string;
	/** @format int32 */
	libraryLocationId: number;
	libraryLocationPath: string;
	/** @format int64 */
	mediaSize: number;
	outdated: boolean;
	/** @format int32 */
	plexServerId: number;
	/** @format date-time */
	scannedAt: string;
	/** @format int32 */
	seasonCount: number;
	/** @format date-time */
	syncedAt: string;
	title: string;
	type: PlexMediaType;
	/** @format date-time */
	updatedAt: string;
	/** @format guid */
	uuid: string;
}

export interface PlexMediaDTO {
	/** @format date-time */
	addedAt: string;
	/** @format int32 */
	childCount: number;
	children: PlexMediaDTO[];
	contentRating: string;
	/** @format int32 */
	duration: number;
	fullThumbUrl: string;
	hasArt: boolean;
	hasBanner: boolean;
	hasTheme: boolean;
	hasThumb: boolean;
	/** @format int32 */
	id: number;
	/** @format int32 */
	index: number;
	/** @format int32 */
	key: number;
	mediaData: PlexMediaDataDTO[];
	/** @format int64 */
	mediaSize: number;
	/** @format date-time */
	originallyAvailableAt?: string | null;
	/** @format int32 */
	plexLibraryId: number;
	/** @format int32 */
	plexServerId: number;
	qualities: PlexMediaQualityDTO[];
	/** @format double */
	rating: number;
	sortTitle: string;
	studio: string;
	summary: string;
	title: string;
	/** @format int32 */
	tvShowId: number;
	/** @format int32 */
	tvShowSeasonId: number;
	type: PlexMediaType;
	/** @format date-time */
	updatedAt: string;
	/** @format int32 */
	year: number;
}

export interface PlexMediaDataDTO {
	/** @format double */
	aspectRatio: number;
	/** @format int32 */
	audioChannels: number;
	audioCodec: string;
	audioProfile: string;
	/** @format int32 */
	bitrate: number;
	/** @format int64 */
	duration: number;
	/** @format int32 */
	height: number;
	mediaFormat: string;
	parts: PlexMediaDataPartDTO[];
	videoCodec: string;
	videoFrameRate: string;
	videoProfile: string;
	videoResolution: string;
	/** @format int32 */
	width: number;
}

export interface PlexMediaDataPartDTO {
	container: string;
	/** @format int32 */
	duration: number;
	file: string;
	obfuscatedFilePath: string;
	/** @format int64 */
	size: number;
	videoProfile: string;
}

export interface PlexMediaQualityDTO {
	displayQuality: string;
	hashId: string;
	quality: string;
}

export interface PlexMediaSlimDTO {
	/** @format date-time */
	addedAt: string;
	/** @format int32 */
	childCount: number;
	children: PlexMediaSlimDTO[];
	/** @format int32 */
	duration: number;
	fullThumbUrl: string;
	hasThumb: boolean;
	/** @format int32 */
	id: number;
	/** @format int32 */
	index: number;
	/** @format int64 */
	mediaSize: number;
	/** @format int32 */
	plexLibraryId: number;
	/** @format int32 */
	plexServerId: number;
	qualities: PlexMediaQualityDTO[];
	sortTitle: string;
	title: string;
	type: PlexMediaType;
	/** @format date-time */
	updatedAt: string;
	/** @format int32 */
	year: number;
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
	plexLibraryIds: number[];
	/** @format int32 */
	plexServerId: number;
}

export interface PlexServerConnectionDTO {
	iPv4: boolean;
	iPv6: boolean;
	address: string;
	/** @format int32 */
	id: number;
	latestConnectionStatus?: PlexServerStatusDTO | null;
	local: boolean;
	/** @format int32 */
	plexServerId: number;
	/** @format int32 */
	port: number;
	portFix: boolean;
	progress: ServerConnectionCheckStatusProgressDTO;
	protocol: string;
	relay: boolean;
	serverStatusList: PlexServerStatusDTO[];
	url: string;
}

export interface PlexServerDTO {
	/** @format date-time */
	createdAt: string;
	device: string;
	dnsRebindingProtection: boolean;
	home: boolean;
	httpsRequired: boolean;
	/** @format int32 */
	id: number;
	/** @format date-time */
	lastSeenAt: string;
	machineIdentifier: string;
	name: string;
	natLoopbackSupported: boolean;
	owned: boolean;
	/** @format int32 */
	ownerId: number;
	platform: string;
	platformVersion: string;
	plexServerConnections: PlexServerConnectionDTO[];
	plexServerOwnerUsername: string;
	/** @format int32 */
	preferredConnectionId: number;
	presence: boolean;
	product: string;
	productVersion: string;
	provides: string;
	publicAddress: string;
	publicAddressMatches: boolean;
	relay: boolean;
	synced: boolean;
}

export interface PlexServerSettingsModel {
	/** @format int32 */
	downloadSpeedLimit: number;
	machineIdentifier?: string | null;
	plexServerName?: string | null;
}

export interface PlexServerStatusDTO {
	/** @format int32 */
	id: number;
	isSuccessful: boolean;
	/** @format date-time */
	lastChecked: string;
	/** @format int32 */
	plexServerConnectionId: number;
	/** @format int32 */
	plexServerId: number;
	/** @format int32 */
	statusCode: number;
	statusMessage: string;
}

export interface ReasonDTO {
	message: string;
	metadata: Record<string, any>;
}

export interface ResultDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
}

export interface ResultDTOOfAuthPin {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: AuthPin | null;
}

export interface ResultDTOOfBoolean {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value: boolean;
}

export interface ResultDTOOfDownloadTaskDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: DownloadTaskDTO | null;
}

export interface ResultDTOOfFileSystemDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: FileSystemDTO | null;
}

export interface ResultDTOOfFolderPathDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: FolderPathDTO | null;
}

export interface ResultDTOOfInt32 {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	/** @format int32 */
	value: number;
}

export interface ResultDTOOfListOfDownloadPreviewDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: DownloadPreviewDTO[] | null;
}

export interface ResultDTOOfListOfFolderPathDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: FolderPathDTO[] | null;
}

export interface ResultDTOOfListOfNotificationDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: NotificationDTO[] | null;
}

export interface ResultDTOOfListOfPlexAccountDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: PlexAccountDTO[] | null;
}

export interface ResultDTOOfListOfPlexLibraryDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: PlexLibraryDTO[] | null;
}

export interface ResultDTOOfListOfPlexMediaSlimDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: PlexMediaSlimDTO[] | null;
}

export interface ResultDTOOfListOfPlexServerConnectionDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: PlexServerConnectionDTO[] | null;
}

export interface ResultDTOOfListOfPlexServerDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: PlexServerDTO[] | null;
}

export interface ResultDTOOfListOfPlexServerStatusDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: PlexServerStatusDTO[] | null;
}

export interface ResultDTOOfListOfServerDownloadProgressDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: ServerDownloadProgressDTO[] | null;
}

export interface ResultDTOOfPlexAccountDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: PlexAccountDTO | null;
}

export interface ResultDTOOfPlexLibraryDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: PlexLibraryDTO | null;
}

export interface ResultDTOOfPlexMediaDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: PlexMediaDTO | null;
}

export interface ResultDTOOfPlexServerConnectionDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: PlexServerConnectionDTO | null;
}

export interface ResultDTOOfPlexServerDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: PlexServerDTO | null;
}

export interface ResultDTOOfPlexServerStatusDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: PlexServerStatusDTO | null;
}

export interface ResultDTOOfSettingsModelDTO {
	errors: ErrorDTO[];
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	successes: SuccessDTO[];
	value?: SettingsModelDTO | null;
}

export interface ServerConnectionCheckStatusProgressDTO {
	completed: boolean;
	connectionSuccessful: boolean;
	message: string;
	/** @format int32 */
	plexServerConnectionId: number;
	/** @format int32 */
	plexServerId: number;
	/** @format int32 */
	retryAttemptCount: number;
	/** @format int32 */
	retryAttemptIndex: number;
	/** @format int32 */
	statusCode: number;
	/** @format int32 */
	timeToNextRetry: number;
}

export interface ServerDownloadProgressDTO {
	/** @format int32 */
	downloadableTasksCount: number;
	downloads: DownloadProgressDTO[];
	/** @format int32 */
	id: number;
}

export interface ServerSettingsDTO {
	data: PlexServerSettingsModel[];
}

export interface SettingsModelDTO {
	confirmationSettings: ConfirmationSettingsDTO;
	dateTimeSettings: DateTimeSettingsDTO;
	debugSettings: DebugSettingsDTO;
	displaySettings: DisplaySettingsDTO;
	downloadManagerSettings: DownloadManagerSettingsDTO;
	generalSettings: GeneralSettingsDTO;
	languageSettings: LanguageSettingsDTO;
	serverSettings: ServerSettingsDTO;
}

export interface SuccessDTO {
	message: string;
	metadata: Record<string, any>;
}

export interface SyncServerProgress {
	/** @format int32 */
	id: number;
	libraryProgresses: LibraryProgress[];
	/** @format decimal */
	percentage: number;
}

export interface UpdateFolderPathEndpointRequest {
	folderPathDto: FolderPathDTO;
}

export interface UpdatePlexAccountByIdEndpointRequest {
	plexAccountDTO: UpdatePlexAccountDTO;
}

export interface UpdatePlexAccountDTO {
	/** @minLength 1 */
	displayName: string;
	/**
	 * @format int32
	 * @min 0
	 * @exclusiveMin true
	 */
	id: number;
	isEnabled: boolean;
	isMain: boolean;
	/** @minLength 5 */
	password: string;
	/** @minLength 5 */
	username: string;
}

export interface UpdateUserSettingsEndpointRequest {
	settingsModelDto: SettingsModelDTO;
}

export interface ValidatePlexAccountEndpointRequest {
	plexAccount: PlexAccountDTO;
}

export enum ViewMode {
	Table = 'Table',
	Poster = 'Poster',
}

import type { AxiosInstance, AxiosRequestConfig, AxiosResponse, HeadersDefaults, ResponseType } from 'axios';
import axios from 'axios';

export type QueryParamsType = Record<string | number, any>;

export interface FullRequestParams extends Omit<AxiosRequestConfig, 'data' | 'params' | 'url' | 'responseType'> {
	/** set parameter to `true` for call `securityWorker` for this request */
	secure?: boolean;
	/** request path */
	path: string;
	/** content type of request body */
	type?: ContentType;
	/** query params */
	query?: QueryParamsType;
	/** format of response (i.e. response.json() -> format: "json") */
	format?: ResponseType;
	/** request body */
	body?: unknown;
}

export type RequestParams = Omit<FullRequestParams, 'body' | 'method' | 'query' | 'path'>;

export interface ApiConfig<SecurityDataType = unknown> extends Omit<AxiosRequestConfig, 'data' | 'cancelToken'> {
	securityWorker?: (securityData: SecurityDataType | null) => Promise<AxiosRequestConfig | void> | AxiosRequestConfig | void;
	secure?: boolean;
	format?: ResponseType;
}

export enum ContentType {
	Json = 'application/json',
	FormData = 'multipart/form-data',
	UrlEncoded = 'application/x-www-form-urlencoded',
	Text = 'text/plain',
}

export class HttpClient<SecurityDataType = unknown> {
	public instance: AxiosInstance;
	private securityData: SecurityDataType | null = null;
	private securityWorker?: ApiConfig<SecurityDataType>['securityWorker'];
	private secure?: boolean;
	private format?: ResponseType;

	constructor({ securityWorker, secure, format, ...axiosConfig }: ApiConfig<SecurityDataType> = {}) {
		this.instance = axios.create({ ...axiosConfig, baseURL: axiosConfig.baseURL || 'http://localhost:5000' });
		this.secure = secure;
		this.format = format;
		this.securityWorker = securityWorker;
	}

	public setSecurityData = (data: SecurityDataType | null) => {
		this.securityData = data;
	};

	protected mergeRequestParams(params1: AxiosRequestConfig, params2?: AxiosRequestConfig): AxiosRequestConfig {
		const method = params1.method || (params2 && params2.method);

		return {
			...this.instance.defaults,
			...params1,
			...(params2 || {}),
			headers: {
				...((method && this.instance.defaults.headers[method.toLowerCase() as keyof HeadersDefaults]) || {}),
				...(params1.headers || {}),
				...((params2 && params2.headers) || {}),
			},
		};
	}

	protected stringifyFormItem(formItem: unknown) {
		if (typeof formItem === 'object' && formItem !== null) {
			return JSON.stringify(formItem);
		} else {
			return `${formItem}`;
		}
	}

	protected createFormData(input: Record<string, unknown>): FormData {
		return Object.keys(input || {}).reduce((formData, key) => {
			const property = input[key];
			const propertyContent: any[] = property instanceof Array ? property : [property];

			for (const formItem of propertyContent) {
				const isFileType = formItem instanceof Blob || formItem instanceof File;
				formData.append(key, isFileType ? formItem : this.stringifyFormItem(formItem));
			}

			return formData;
		}, new FormData());
	}

	public request = async <T = any, _E = any>({
		secure,
		path,
		type,
		query,
		format,
		body,
		...params
	}: FullRequestParams): Promise<AxiosResponse<T>> => {
		const secureParams =
			((typeof secure === 'boolean' ? secure : this.secure) &&
				this.securityWorker &&
				(await this.securityWorker(this.securityData))) ||
			{};
		const requestParams = this.mergeRequestParams(params, secureParams);
		const responseFormat = format || this.format || undefined;

		if (type === ContentType.FormData && body && body !== null && typeof body === 'object') {
			body = this.createFormData(body as Record<string, unknown>);
		}

		if (type === ContentType.Text && body && body !== null && typeof body !== 'string') {
			body = JSON.stringify(body);
		}

		return this.instance.request({
			...requestParams,
			headers: {
				...(requestParams.headers || {}),
				...(type && type !== ContentType.FormData ? { 'Content-Type': type } : {}),
			},
			params: query,
			responseType: responseFormat,
			data: body,
			url: path,
		});
	};
}

/**
 * @title [FastEndpoints] PlexRipper Swagger Internal API
 * @version v1
 * @baseUrl http://localhost:5000
 */
export class GeneratedApiClient<SecurityDataType extends unknown> extends HttpClient<SecurityDataType> {
	folderPath = {
		/**
		 * No description
		 *
		 * @tags Folderpath
		 * @name CreateFolderPathEndpoint
		 * @request POST:/api/FolderPath/
		 */
		createFolderPathEndpoint: (data: FolderPathDTO, params: RequestParams = {}) =>
			this.request<ResultDTOOfFolderPathDTO, ResultDTO>({
				path: `/api/FolderPath/`,
				method: 'POST',
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Folderpath
		 * @name GetAllFolderPathsEndpoint
		 * @request GET:/api/FolderPath/
		 */
		getAllFolderPathsEndpoint: (params: RequestParams = {}) =>
			this.request<ResultDTOOfListOfFolderPathDTO, any>({
				path: `/api/FolderPath/`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Folderpath
		 * @name UpdateFolderPathEndpoint
		 * @request PUT:/api/FolderPath/
		 */
		updateFolderPathEndpoint: (data: FolderPathDTO, params: RequestParams = {}) =>
			this.request<ResultDTOOfFolderPathDTO, ResultDTO>({
				path: `/api/FolderPath/`,
				method: 'PUT',
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Folderpath
		 * @name DeleteFolderPathEndpoint
		 * @request DELETE:/api/FolderPath/{id}
		 */
		deleteFolderPathEndpoint: (id: number, params: RequestParams = {}) =>
			this.request<ResultDTO, ResultDTO>({
				path: `/api/FolderPath/${id}`,
				method: 'DELETE',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Folderpath
		 * @name GetFolderPathDirectoryEndpoint
		 * @summary Get all the FolderPaths entities in the database
		 * @request GET:/api/FolderPath/directory
		 */
		getFolderPathDirectoryEndpoint: (
			query: {
				path: string;
			},
			params: RequestParams = {},
		) =>
			this.request<ResultDTOOfFileSystemDTO, ResultDTO>({
				path: `/api/FolderPath/directory`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),
	};
	notification = {
		/**
		 * No description
		 *
		 * @tags Notification
		 * @name ClearAllNotificationsEndpoint
		 * @request DELETE:/api/Notification/clear
		 */
		clearAllNotificationsEndpoint: (params: RequestParams = {}) =>
			this.request<ResultDTOOfInt32, ResultDTO>({
				path: `/api/Notification/clear`,
				method: 'DELETE',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Notification
		 * @name GetAllNotificationsEndpoint
		 * @request GET:/api/Notification/
		 */
		getAllNotificationsEndpoint: (params: RequestParams = {}) =>
			this.request<ResultDTOOfListOfNotificationDTO, ResultDTO>({
				path: `/api/Notification/`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Notification
		 * @name HideNotificationEndpoint
		 * @request PUT:/api/Notification/{notificationId}
		 */
		hideNotificationEndpoint: (notificationId: number, params: RequestParams = {}) =>
			this.request<ResultDTO, ErrorResponse | ResultDTO>({
				path: `/api/Notification/${notificationId}`,
				method: 'PUT',
				format: 'json',
				...params,
			}),
	};
	plexAccount = {
		/**
		 * No description
		 *
		 * @tags Plexaccount
		 * @name CreatePlexAccountEndpoint
		 * @request POST:/api/PlexAccount/
		 */
		createPlexAccountEndpoint: (data: PlexAccountDTO, params: RequestParams = {}) =>
			this.request<ResultDTO, ResultDTO>({
				path: `/api/PlexAccount/`,
				method: 'POST',
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexaccount
		 * @name DeletePlexAccountByIdEndpoint
		 * @request DELETE:/api/PlexAccount/{plexAccountId}
		 */
		deletePlexAccountByIdEndpoint: (plexAccountId: number, params: RequestParams = {}) =>
			this.request<ResultDTO, ResultDTO>({
				path: `/api/PlexAccount/${plexAccountId}`,
				method: 'DELETE',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexaccount
		 * @name GetPlexAccountByIdEndpoint
		 * @request GET:/api/PlexAccount/{plexAccountId}
		 */
		getPlexAccountByIdEndpoint: (plexAccountId: number, params: RequestParams = {}) =>
			this.request<ResultDTOOfPlexAccountDTO, ResultDTO>({
				path: `/api/PlexAccount/${plexAccountId}`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexaccount
		 * @name UpdatePlexAccountByIdEndpoint
		 * @request PUT:/api/PlexAccount/{plexAccountId}
		 */
		updatePlexAccountByIdEndpoint: (
			plexAccountId: number,
			query: {
				inspect: boolean;
			},
			data: UpdatePlexAccountDTO,
			params: RequestParams = {},
		) =>
			this.request<ResultDTOOfPlexAccountDTO, ErrorResponse | ResultDTO>({
				path: `/api/PlexAccount/${plexAccountId}`,
				method: 'PUT',
				query: query,
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexaccount
		 * @name GetAllPlexAccountsEndpoint
		 * @request GET:/api/PlexAccount
		 */
		getAllPlexAccountsEndpoint: (
			query?: {
				/** @default false */
				enabledOnly?: boolean;
			},
			params: RequestParams = {},
		) =>
			this.request<ResultDTOOfListOfPlexAccountDTO, ResultDTO>({
				path: `/api/PlexAccount`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexaccount
		 * @name IsUsernameAvailableEndpoint
		 * @request GET:/api/PlexAccount/check
		 */
		isUsernameAvailableEndpoint: (
			query: {
				username: string;
			},
			params: RequestParams = {},
		) =>
			this.request<ResultDTOOfBoolean, ErrorResponse | ResultDTO>({
				path: `/api/PlexAccount/check`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexaccount
		 * @name RefreshPlexAccountAccessEndpoint
		 * @request GET:/api/PlexAccount/refresh/PlexAccountId
		 */
		refreshPlexAccountAccessEndpoint: (
			query: {
				/** @format int32 */
				plexAccountId: number;
			},
			params: RequestParams = {},
		) =>
			this.request<ResultDTO, ErrorResponse | ResultDTO>({
				path: `/api/PlexAccount/refresh/PlexAccountId`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexaccount
		 * @name ValidatePlexAccountEndpoint
		 * @request POST:/api/PlexAccount/validate
		 */
		validatePlexAccountEndpoint: (data: PlexAccountDTO, params: RequestParams = {}) =>
			this.request<ResultDTOOfPlexAccountDTO, ResultDTO>({
				path: `/api/PlexAccount/validate`,
				method: 'POST',
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexaccount
		 * @name Verify2FaPinEndpoint
		 * @request GET:/api/PlexAccount/authpin
		 */
		verify2FaPinEndpoint: (
			query: {
				/**
				 * @format int32
				 * @default 0
				 */
				authPinId?: number;
				clientId: string;
			},
			params: RequestParams = {},
		) =>
			this.request<ResultDTOOfAuthPin, ResultDTO>({
				path: `/api/PlexAccount/authpin`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),
	};
	download = {
		/**
		 * No description
		 *
		 * @tags Download
		 * @name ClearCompletedDownloadTasksEndpoint
		 * @request POST:/api/Download/clear
		 */
		clearCompletedDownloadTasksEndpoint: (data: string[], params: RequestParams = {}) =>
			this.request<ResultDTO, any>({
				path: `/api/Download/clear`,
				method: 'POST',
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Download
		 * @name CreateDownloadTasksEndpoint
		 * @request POST:/api/Download/download
		 */
		createDownloadTasksEndpoint: (data: DownloadMediaDTO[], params: RequestParams = {}) =>
			this.request<ResultDTO, ResultDTO>({
				path: `/api/Download/download`,
				method: 'POST',
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Download
		 * @name DeleteDownloadTaskEndpoint
		 * @request DELETE:/api/Download/delete
		 */
		deleteDownloadTaskEndpoint: (data: string[], params: RequestParams = {}) =>
			this.request<ResultDTO, ResultDTO>({
				path: `/api/Download/delete`,
				method: 'DELETE',
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Download
		 * @name GetDownloadTaskByGuidEndpoint
		 * @request GET:/api/Download/detail/{downloadTaskGuid}
		 */
		getDownloadTaskByGuidEndpoint: (
			downloadTaskGuid: string,
			query?: {
				/** @default 0 */
				type?: DownloadTaskType;
			},
			params: RequestParams = {},
		) =>
			this.request<ResultDTOOfDownloadTaskDTO, ResultDTO>({
				path: `/api/Download/detail/${downloadTaskGuid}`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Download
		 * @name GetAllDownloadTasksEndpoint
		 * @request GET:/api/Download
		 */
		getAllDownloadTasksEndpoint: (params: RequestParams = {}) =>
			this.request<ResultDTOOfListOfServerDownloadProgressDTO, ResultDTO>({
				path: `/api/Download`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Download
		 * @name PauseDownloadTaskEndpoint
		 * @request GET:/api/Download/pause/{downloadTaskGuid}
		 */
		pauseDownloadTaskEndpoint: (downloadTaskGuid: string, params: RequestParams = {}) =>
			this.request<ResultDTO, ResultDTO>({
				path: `/api/Download/pause/${downloadTaskGuid}`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Download
		 * @name GetDownloadPreviewEndpoint
		 * @request POST:/api/Download/preview
		 */
		getDownloadPreviewEndpoint: (data: DownloadMediaDTO[], params: RequestParams = {}) =>
			this.request<ResultDTOOfListOfDownloadPreviewDTO, ResultDTO>({
				path: `/api/Download/preview`,
				method: 'POST',
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Download
		 * @name RestartDownloadTaskEndpoint
		 * @request GET:/api/Download/restart/{downloadTaskGuid}
		 */
		restartDownloadTaskEndpoint: (downloadTaskGuid: string, params: RequestParams = {}) =>
			this.request<ResultDTO, ResultDTO>({
				path: `/api/Download/restart/${downloadTaskGuid}`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Download
		 * @name StartDownloadTaskEndpoint
		 * @request GET:/api/Download/start/{downloadTaskGuid}
		 */
		startDownloadTaskEndpoint: (downloadTaskGuid: string, params: RequestParams = {}) =>
			this.request<ResultDTO, ErrorResponse | ResultDTO>({
				path: `/api/Download/start/${downloadTaskGuid}`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Download
		 * @name StopDownloadTaskEndpoint
		 * @request GET:/api/Download/stop/{DownloadTaskId}
		 */
		stopDownloadTaskEndpoint: (
			downloadTaskId: string,
			query: {
				/** @format guid */
				downloadTaskGuid: string;
			},
			params: RequestParams = {},
		) =>
			this.request<ResultDTO, ResultDTO>({
				path: `/api/Download/stop/${downloadTaskId}`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),
	};
	plexLibrary = {
		/**
		 * No description
		 *
		 * @tags Plexlibrary
		 * @name GetPlexLibraryByIdEndpoint
		 * @request GET:/api/PlexLibrary/{plexLibraryId}
		 */
		getPlexLibraryByIdEndpoint: (plexLibraryId: number, params: RequestParams = {}) =>
			this.request<ResultDTOOfPlexLibraryDTO, ResultDTO>({
				path: `/api/PlexLibrary/${plexLibraryId}`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexlibrary
		 * @name GetAllPlexLibrariesEndpoint
		 * @request GET:/api/PlexLibrary/
		 */
		getAllPlexLibrariesEndpoint: (params: RequestParams = {}) =>
			this.request<ResultDTOOfListOfPlexLibraryDTO, ResultDTO>({
				path: `/api/PlexLibrary/`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexlibrary
		 * @name GetPlexLibraryMediaEndpoint
		 * @request GET:/api/PlexLibrary/{plexLibraryId}/media
		 */
		getPlexLibraryMediaEndpoint: (
			plexLibraryId: number,
			query: {
				/**
				 * @format int32
				 * @default 0
				 */
				page: number;
				/**
				 * @format int32
				 * @default 0
				 */
				size: number;
			},
			params: RequestParams = {},
		) =>
			this.request<ResultDTOOfListOfPlexMediaSlimDTO, ResultDTO>({
				path: `/api/PlexLibrary/${plexLibraryId}/media`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexlibrary
		 * @name RefreshLibraryMediaEndpoint
		 * @request GET:/api/PlexLibrary/refresh/{plexLibraryId}
		 */
		refreshLibraryMediaEndpoint: (plexLibraryId: number, params: RequestParams = {}) =>
			this.request<ResultDTOOfPlexLibraryDTO, ResultDTO>({
				path: `/api/PlexLibrary/refresh/${plexLibraryId}`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexlibrary
		 * @name SetPlexLibraryDefaultDestinationByIdEndpoint
		 * @request PUT:/api/PlexLibrary/{plexLibraryId}/default/destination/{folderPathId}
		 */
		setPlexLibraryDefaultDestinationByIdEndpoint: (plexLibraryId: number, folderPathId: number, params: RequestParams = {}) =>
			this.request<ResultDTO, ErrorResponse | ResultDTO>({
				path: `/api/PlexLibrary/${plexLibraryId}/default/destination/${folderPathId}`,
				method: 'PUT',
				format: 'json',
				...params,
			}),
	};
	plexMedia = {
		/**
		 * No description
		 *
		 * @tags Plexmedia
		 * @name GetMediaDetailByIdEndpoint
		 * @request GET:/api/PlexMedia/detail/{plexMediaId}
		 */
		getMediaDetailByIdEndpoint: (
			plexMediaId: number,
			query: {
				type: PlexMediaType;
			},
			params: RequestParams = {},
		) =>
			this.request<ResultDTOOfPlexMediaDTO, ResultDTO>({
				path: `/api/PlexMedia/detail/${plexMediaId}`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexmedia
		 * @name GetThumbnailImageEndpoint
		 * @request GET:/api/PlexMedia/thumb
		 */
		getThumbnailImageEndpoint: (
			query: {
				/**
				 * @format int32
				 * @default 0
				 */
				height?: number;
				/** @format int32 */
				mediaId: number;
				mediaType: PlexMediaType;
				/**
				 * @format int32
				 * @default 0
				 */
				width?: number;
			},
			params: RequestParams = {},
		) =>
			this.request<ResultDTO, ResultDTO>({
				path: `/api/PlexMedia/thumb`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),
	};
	plexServerConnection = {
		/**
		 * No description
		 *
		 * @tags Plexserverconnection
		 * @name CheckAllConnectionsStatusByPlexServerEndpoint
		 * @request GET:/api/PlexServerConnection/check/by-server/{plexServerId}
		 */
		checkAllConnectionsStatusByPlexServerEndpoint: (plexServerId: number, params: RequestParams = {}) =>
			this.request<ResultDTOOfListOfPlexServerStatusDTO, ResultDTO>({
				path: `/api/PlexServerConnection/check/by-server/${plexServerId}`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexserverconnection
		 * @name CheckConnectionStatusByIdEndpoint
		 * @request GET:/api/PlexServerConnection/check/{plexServerConnectionId}
		 */
		checkConnectionStatusByIdEndpoint: (plexServerConnectionId: number, params: RequestParams = {}) =>
			this.request<ResultDTOOfPlexServerStatusDTO, ResultDTO>({
				path: `/api/PlexServerConnection/check/${plexServerConnectionId}`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexserverconnection
		 * @name GetPlexServerConnectionByIdEndpoint
		 * @request GET:/api/PlexServerConnection/{plexServerConnectionId}
		 */
		getPlexServerConnectionByIdEndpoint: (plexServerConnectionId: number, params: RequestParams = {}) =>
			this.request<ResultDTOOfPlexServerConnectionDTO, ResultDTO>({
				path: `/api/PlexServerConnection/${plexServerConnectionId}`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexserverconnection
		 * @name GetAllPlexServerConnectionsEndpoint
		 * @request GET:/api/PlexServerConnection/
		 */
		getAllPlexServerConnectionsEndpoint: (params: RequestParams = {}) =>
			this.request<ResultDTOOfListOfPlexServerConnectionDTO, ResultDTO>({
				path: `/api/PlexServerConnection/`,
				method: 'GET',
				format: 'json',
				...params,
			}),
	};
	plexServer = {
		/**
		 * No description
		 *
		 * @tags Plexserver
		 * @name SetPreferredPlexServerConnectionEndpoint
		 * @request GET:/api/PlexServer/{plexServerId}/preferred-connection/{plexServerConnectionId}
		 */
		setPreferredPlexServerConnectionEndpoint: (
			plexServerId: number,
			plexServerConnectionId: number,
			params: RequestParams = {},
		) =>
			this.request<ResultDTO, ResultDTO>({
				path: `/api/PlexServer/${plexServerId}/preferred-connection/${plexServerConnectionId}`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexserver
		 * @name GetPlexServerByIdEndpoint
		 * @request GET:/api/PlexServer/{plexServerId}
		 */
		getPlexServerByIdEndpoint: (plexServerId: number, params: RequestParams = {}) =>
			this.request<ResultDTOOfPlexServerDTO, ErrorResponse | ResultDTO>({
				path: `/api/PlexServer/${plexServerId}`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * @description  Retrieves all the PlexServers, without PlexLibraries but with all its connections currently in the database.
		 *
		 * @tags Plexserver
		 * @name GetAllPlexServersEndpoint
		 * @summary Get All the PlexServers, without PlexLibraries but with all its connections.
		 * @request GET:/api/PlexServer/
		 */
		getAllPlexServersEndpoint: (params: RequestParams = {}) =>
			this.request<ResultDTOOfListOfPlexServerDTO, ResultDTO>({
				path: `/api/PlexServer/`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexserver
		 * @name QueueInspectPlexServerJobEndpoint
		 * @request GET:/api/PlexServer/{plexServerId}/inspect
		 */
		queueInspectPlexServerJobEndpoint: (plexServerId: number, params: RequestParams = {}) =>
			this.request<ResultDTO, ErrorResponse | ResultDTO>({
				path: `/api/PlexServer/${plexServerId}/inspect`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexserver
		 * @name RefreshPlexServerConnectionsEndpoint
		 * @request GET:/api/PlexServer/{plexServerId}/refresh
		 */
		refreshPlexServerConnectionsEndpoint: (plexServerId: number, params: RequestParams = {}) =>
			this.request<ResultDTOOfPlexServerDTO, ErrorResponse | ResultDTO>({
				path: `/api/PlexServer/${plexServerId}/refresh`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Plexserver
		 * @name QueueSyncPlexServerJobEndpoint
		 * @request GET:/api/PlexServer/{plexServerId}/sync
		 */
		queueSyncPlexServerJobEndpoint: (
			plexServerId: number,
			query?: {
				/** @default false */
				forceSync?: boolean;
			},
			params: RequestParams = {},
		) =>
			this.request<ResultDTO, ResultDTO>({
				path: `/api/PlexServer/${plexServerId}/sync`,
				method: 'GET',
				query: query,
				format: 'json',
				...params,
			}),
	};
	settings = {
		/**
		 * No description
		 *
		 * @tags Settings
		 * @name GetUserSettingsEndpoint
		 * @request GET:/api/Settings/
		 */
		getUserSettingsEndpoint: (params: RequestParams = {}) =>
			this.request<ResultDTOOfSettingsModelDTO, ResultDTO>({
				path: `/api/Settings/`,
				method: 'GET',
				format: 'json',
				...params,
			}),

		/**
		 * No description
		 *
		 * @tags Settings
		 * @name UpdateUserSettingsEndpoint
		 * @request PUT:/api/Settings/
		 */
		updateUserSettingsEndpoint: (data: SettingsModelDTO, params: RequestParams = {}) =>
			this.request<ResultDTOOfSettingsModelDTO, ResultDTO>({
				path: `/api/Settings/`,
				method: 'PUT',
				body: data,
				type: ContentType.Json,
				format: 'json',
				...params,
			}),
	};
}
