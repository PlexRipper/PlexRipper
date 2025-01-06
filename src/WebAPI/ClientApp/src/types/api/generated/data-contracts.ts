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

/** @example {"username":"PlexRipperRocks","password":"Pl€XR!ℙℙ€R69","rememberMe":false} */
export interface AppUserLoginEndpointRequest {
  /**
   * @minLength 1
   * @default "Pl€XR!ℙℙ€R69"
   */
  password: string;
  /** @default false */
  rememberMe: boolean;
  /**
   * @minLength 1
   * @default "PlexRipperRocks"
   */
  username: string;
}

export interface CheckAllConnectionStatusUpdateDTO {
  plexServersWithConnectionIds: Record<string, number[]>;
}

export interface ConfirmationSettingsDTO {
  askDownloadEpisodeConfirmation: boolean;
  askDownloadMovieConfirmation: boolean;
  askDownloadSeasonConfirmation: boolean;
  askDownloadTvShowConfirmation: boolean;
}

export interface CreateDownloadTasksRequest {
  customDestinationFolderPath: string;
  /** @format int32 */
  destinationFolderPathId?: number | null;
  downloadMedias: DownloadMediaDTO[];
}

export interface CreatePlexServerConnectionEndpointRequest {
  /** @minLength 1 */
  address: string;
  /**
   * @format int32
   * @min 0
   * @exclusiveMin true
   */
  plexServerId: number;
  /**
   * @format int32
   * @min 0
   * @exclusiveMin true
   */
  port: number;
  /** @minLength 1 */
  protocol: string;
  /** @minLength 1 */
  url: string;
}

export enum DataType {
  PlexAccount = "PlexAccount",
  PlexServer = "PlexServer",
  PlexLibrary = "PlexLibrary",
  PlexServerConnection = "PlexServerConnection",
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

export interface DisplaySettingsDTO {
  allOverviewViewMode: PlexMediaType;
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
  Unknown = "Unknown",
  Error = "Error",
  Queued = "Queued",
  Downloading = "Downloading",
  DownloadFinished = "DownloadFinished",
  Paused = "Paused",
  Stopped = "Stopped",
  Deleted = "Deleted",
  Merging = "Merging",
  Moving = "Moving",
  MergePaused = "MergePaused",
  MovePaused = "MovePaused",
  MergeFinished = "MergeFinished",
  MoveFinished = "MoveFinished",
  Completed = "Completed",
  ServerUnreachable = "ServerUnreachable",
  MoveError = "MoveError",
  MergeError = "MergeError",
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
  None = "None",
  Movie = "Movie",
  MovieData = "MovieData",
  MoviePart = "MoviePart",
  TvShow = "TvShow",
  Season = "Season",
  Episode = "Episode",
  EpisodeData = "EpisodeData",
  EpisodePart = "EpisodePart",
}

export interface DownloadWorkerLogDTO {
  /** @format date-time */
  createdAt: string;
  /** @format guid */
  downloadTaskId: string;
  /** @format int32 */
  downloadWorkerTaskId: number;
  logLevel: NotificationLevel;
  message: string;
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

export interface FileSystemDTO {
  current?: FileSystemModelDTO | null;
  directories: FileSystemModelDTO[];
  files: FileSystemModelDTO[];
  parent: string;
}

export enum FileSystemEntityType {
  Parent = "Parent",
  Drive = "Drive",
  Folder = "Folder",
  File = "File",
}

export interface FileSystemModelDTO {
  extension: string;
  hasReadPermission: boolean;
  hasWritePermission: boolean;
  /** @format date-time */
  lastModified?: string | null;
  name: string;
  path: string;
  /** @format int64 */
  size: number;
  type: FileSystemEntityType;
}

export interface FolderPathDTO {
  directory: string;
  displayName: string;
  folderType: FolderType;
  /** @format int32 */
  id: number;
  isDefault: boolean;
  isValid: boolean;
  mediaType: PlexMediaType;
}

export enum FolderType {
  None = "None",
  DownloadFolder = "DownloadFolder",
  MovieFolder = "MovieFolder",
  TvShowFolder = "TvShowFolder",
  MusicFolder = "MusicFolder",
  PhotosFolder = "PhotosFolder",
  OtherVideosFolder = "OtherVideosFolder",
  GamesVideosFolder = "GamesVideosFolder",
  Unknown = "Unknown",
}

export interface GeneralSettingsDTO {
  /** @format int32 */
  activeAccountId: number;
  disableAnimatedBackground: boolean;
  firstTimeSetup: boolean;
  hasBeenInvitedToDiscord: boolean;
  hideMediaFromOfflineServers: boolean;
  hideMediaFromOwnedServers: boolean;
  useLowQualityPosterImages: boolean;
}

export interface IError {
  reasons?: IError[] | null;
}

export enum JobStatus {
  Started = "Started",
  Completed = "Completed",
}

export interface JobStatusUpdateDTOOfObject {
  data?: any;
  id: string;
  /** @format date-time */
  jobStartTime: string;
  jobType: JobTypes;
  status: JobStatus;
}

export enum JobTypes {
  Unknown = "Unknown",
  CheckAllConnectionsStatusByPlexServerJob = "CheckAllConnectionsStatusByPlexServerJob",
  DownloadJob = "DownloadJob",
  FileMergeJob = "FileMergeJob",
  SyncServerMediaJob = "SyncServerMediaJob",
  InspectPlexServerJob = "InspectPlexServerJob",
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
  /** @format int32 */
  step: number;
  /** @format duration */
  timeRemaining: string;
  /** @format date-time */
  timeStamp: string;
  /** @format int32 */
  total: number;
  /** @format int32 */
  totalSteps: number;
}

/**
 * Message types for SignalR communication from server to client.
 *
 */
export enum MessageTypes {
  LibraryProgress = "LibraryProgress",
  DownloadTaskUpdate = "DownloadTaskUpdate",
  ServerDownloadProgress = "ServerDownloadProgress",
  ServerConnectionCheckStatusProgress = "ServerConnectionCheckStatusProgress",
  FileMergeProgress = "FileMergeProgress",
  SyncServerMediaProgress = "SyncServerMediaProgress",
  Notification = "Notification",
  JobStatusUpdate = "JobStatusUpdate",
  RefreshNotification = "RefreshNotification",
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
  None = "None",
  Verbose = "Verbose",
  Debug = "Debug",
  Information = "Information",
  Success = "Success",
  Warning = "Warning",
  Error = "Error",
  Fatal = "Fatal",
}

export interface PlexAccountDTO {
  is2Fa: boolean;
  authenticationToken: string;
  clientId: string;
  displayName: string;
  email: string;
  hasPassword: boolean;
  /** @format int32 */
  id: number;
  isEnabled: boolean;
  isMain: boolean;
  isValidated: boolean;
  password: string;
  /** @format int64 */
  plexId: number;
  plexLibraryAccess: number[];
  plexServerAccess: number[];
  title: string;
  username: string;
  uuid: string;
  /** @format date-time */
  validatedAt: string;
  verificationCode: string;
}

export interface PlexLibraryDTO {
  /** @format int32 */
  count: number;
  /** @format date-time */
  createdAt: string;
  defaultDestination?: FolderPathDTO | null;
  /** @format int32 */
  defaultDestinationId: number;
  /** @format int32 */
  episodeCount: number;
  /** @format int32 */
  id: number;
  key: string;
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
  syncedAt?: string | null;
  title: string;
  type: PlexMediaType;
  /** @format date-time */
  updatedAt: string;
  uuid: string;
}

export interface PlexMediaDTO {
  /** @format date-time */
  addedAt: string;
  /** @format int32 */
  childCount: number;
  children: PlexMediaDTO[];
  contentRating?: string | null;
  /** @format int32 */
  duration: number;
  /** @format int32 */
  grandChildCount: number;
  hasArt: boolean;
  hasBanner: boolean;
  hasTheme: boolean;
  hasThumb: boolean;
  /** @format int32 */
  id: number;
  /** @format int32 */
  key: number;
  mediaData: PlexMediaDataDTO[];
  /** @format int64 */
  mediaSize: number;
  /** @format int32 */
  metaDataKey: number;
  /** @format date-time */
  originallyAvailableAt?: string | null;
  /** @format int32 */
  plexLibraryId: number;
  /** @format int32 */
  plexServerId: number;
  plexToken: string;
  qualities: PlexMediaQualityDTO[];
  /** @format double */
  rating: number;
  searchTitle: string;
  /** @format int32 */
  sortIndex: number;
  studio: string;
  summary: string;
  title: string;
  /** @format int32 */
  tvShowId: number;
  /** @format int32 */
  tvShowSeasonId: number;
  type: PlexMediaType;
  /** @format date-time */
  updatedAt?: string | null;
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
  /** @format int32 */
  duration: number;
  /** @format int32 */
  grandChildCount: number;
  hasThumb: boolean;
  /** @format int32 */
  id: number;
  /** @format int32 */
  key: number;
  /** @format int64 */
  mediaSize: number;
  /** @format int32 */
  metaDataKey: number;
  /** @format int32 */
  plexLibraryId: number;
  /** @format int32 */
  plexServerId: number;
  plexToken: string;
  qualities: PlexMediaQualityDTO[];
  searchTitle: string;
  /** @format int32 */
  sortIndex: number;
  title: string;
  type: PlexMediaType;
  /** @format date-time */
  updatedAt?: string | null;
  /** @format int32 */
  year: number;
}

export interface PlexMediaStatisticsDTO {
  /** @format int32 */
  episodeCount: number;
  /** @format int32 */
  mediaCount: number;
  mediaList: PlexMediaSlimDTO[];
  /** @format int64 */
  mediaSize: number;
  /** @format int32 */
  movieCount: number;
  /** @format int32 */
  seasonCount: number;
  /** @format int32 */
  tvShowCount: number;
}

export enum PlexMediaType {
  None = "None",
  Movie = "Movie",
  TvShow = "TvShow",
  Season = "Season",
  Episode = "Episode",
  Music = "Music",
  Album = "Album",
  Song = "Song",
  Photos = "Photos",
  OtherVideos = "OtherVideos",
  Games = "Games",
  Unknown = "Unknown",
}

export interface PlexServerConnectionDTO {
  iPv4: boolean;
  iPv6: boolean;
  address: string;
  /** @format int32 */
  id: number;
  isCustom: boolean;
  isPlexTvConnection: boolean;
  latestConnectionStatus?: PlexServerStatusDTO | null;
  local: boolean;
  /** @format int32 */
  plexServerId: number;
  /** @format int32 */
  port: number;
  protocol: string;
  relay: boolean;
  serverStatusList: PlexServerStatusDTO[];
  uri: string;
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
  isEnabled: boolean;
  /** @format date-time */
  lastSeenAt: string;
  machineIdentifier: string;
  name: string;
  natLoopbackSupported: boolean;
  owned: boolean;
  /** @format int64 */
  ownerId: number;
  platform: string;
  platformVersion: string;
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

export interface PlexServerSettingItemModule {
  /** @format int32 */
  downloadSpeedLimit: number;
  hidden: boolean;
  machineIdentifier: string;
  plexServerName: string;
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

export interface ResultDTOOfListOfDownloadWorkerLogDTO {
  errors: ErrorDTO[];
  isFailed: boolean;
  isSuccess: boolean;
  reasons: ReasonDTO[];
  successes: SuccessDTO[];
  value?: DownloadWorkerLogDTO[] | null;
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

export interface ResultDTOOfPlexMediaStatisticsDTO {
  errors: ErrorDTO[];
  isFailed: boolean;
  isSuccess: boolean;
  reasons: ReasonDTO[];
  successes: SuccessDTO[];
  value?: PlexMediaStatisticsDTO | null;
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

export interface ResultDTOOfServerIdentityDTO {
  errors: ErrorDTO[];
  isFailed: boolean;
  isSuccess: boolean;
  reasons: ReasonDTO[];
  successes: SuccessDTO[];
  value?: ServerIdentityDTO | null;
}

export interface ResultDTOOfSettingsModelDTO {
  errors: ErrorDTO[];
  isFailed: boolean;
  isSuccess: boolean;
  reasons: ReasonDTO[];
  successes: SuccessDTO[];
  value?: SettingsModelDTO | null;
}

export interface ResultDTOOfString {
  errors: ErrorDTO[];
  isFailed: boolean;
  isSuccess: boolean;
  reasons: ReasonDTO[];
  successes: SuccessDTO[];
  value?: string | null;
}

export interface ResultDTOOfUserClaimsDTO {
  errors: ErrorDTO[];
  isFailed: boolean;
  isSuccess: boolean;
  reasons: ReasonDTO[];
  successes: SuccessDTO[];
  value?: UserClaimsDTO | null;
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

export interface ServerIdentityDTO {
  claimed: boolean;
  machineIdentifier: string;
  version: string;
}

export interface ServerSettingsDTO {
  data: PlexServerSettingItemModule[];
}

export interface SetNotificationVisibilityEndpointRequest {
  hidden: boolean;
  /**
   * @format int32
   * @min 0
   * @exclusiveMin true
   */
  id: number;
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

export interface SyncServerMediaJobUpdateDTO {
  forceSync: boolean;
  /** @format int32 */
  plexServerId: number;
}

export interface SyncServerMediaProgress {
  /** @format int32 */
  id: number;
  libraryProgresses: LibraryProgress[];
  /** @format decimal */
  percentage: number;
}

export interface UpdatePlexServerConnectionEndpointRequest {
  /** @minLength 1 */
  address: string;
  /**
   * @format int32
   * @min 0
   * @exclusiveMin true
   */
  id: number;
  /**
   * @format int32
   * @min 0
   * @exclusiveMin true
   */
  plexServerId: number;
  /**
   * @format int32
   * @min 0
   * @exclusiveMin true
   */
  port: number;
  /** @minLength 1 */
  protocol: string;
  /** @minLength 1 */
  url: string;
}

export interface UserClaimsDTO {
  claims: string[];
  isLoggedIn: boolean;
  userName: string;
}

export interface ValidatePlexServerConnectionEndpointRequest {
  /** @minLength 1 */
  url: string;
}

export enum ViewMode {
  Poster = "Poster",
  Table = "Table",
}
