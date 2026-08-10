/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

export interface AppCredentialsDTO {
  isDefaultCredentials: boolean;
  password: string;
  userName: string;
}

export interface AppUpdateCheckDTO {
  currentVersion: string;
  isUpdateAvailable: boolean;
  newestVersion: string;
  releaseNotes: ReleaseNoteDTO[];
}

export interface AppUpdateDownloadProgressDTO {
  isComplete: boolean;
  /** @format int32 */
  percentage: number;
}

/** @example {"username":"ReaparrRocks","password":"R€Aℙℙ@rr69","rememberMe":false} */
export interface AppUserLoginEndpointRequest {
  /**
   * @minLength 1
   * @default "R€Aℙℙ@rr69"
   * @example "R€Aℙℙ@rr69"
   */
  password: string;
  /**
   * @default false
   * @example false
   */
  rememberMe: boolean;
  /**
   * @minLength 1
   * @default "ReaparrRocks"
   * @example "ReaparrRocks"
   */
  username: string;
}

export interface BaseResultDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
}

export interface CheckAllConnectionStatusUpdateDTO {
  plexServersWithConnectionIds: Record<string, number[]>;
}

export interface ConfigureRadarrIntegrationRequest {
  /** @minLength 1 */
  apiKey: string;
  /** @minLength 1 */
  url: string;
}

export interface ConfigureSonarrIntegrationRequest {
  /** @minLength 1 */
  apiKey: string;
  /** @minLength 1 */
  url: string;
}

export interface ConfirmationSettingsDTO {
  askDownloadEpisodeConfirmation: boolean;
  askDownloadMovieConfirmation: boolean;
  askDownloadSeasonConfirmation: boolean;
  askDownloadTvShowConfirmation: boolean;
}

export interface CountResponseDTO {
  /** @format int32 */
  count: number;
}

export interface CreateDownloadTasksRequest {
  customDestinationFolderPath: string;
  /** @format int32 */
  destinationFolderPathId?: number | null;
  downloadMedias: DownloadMediaDTO[];
}

export interface CreatePlexAccountEndpointRequest {
  is2Fa: boolean;
  /** @minLength 5 */
  authenticationToken: string;
  /** @minLength 1 */
  clientId: string;
  customAuthenticationToken: string;
  /** @minLength 1 */
  displayName: string;
  email: string;
  isEnabled: boolean;
  isMain: boolean;
  isValidated: boolean;
  /** @minLength 5 */
  password: string;
  /**
   * @format int64
   * @minLength 1
   */
  plexId: number;
  title: string;
  /** @minLength 5 */
  username: string;
  /** @minLength 1 */
  uuid: string;
  /** @format date-time */
  validatedAt: string;
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

export interface DateTimeSettingsDTO {
  longDateFormat: string;
  shortDateFormat: string;
  showRelativeDates: boolean;
  timeFormat: string;
  timeZone: string;
}

export interface DebugSettingsDTO {
  debugModeEnabled: boolean;
  maskAccountNames: boolean;
  maskLibraryNames: boolean;
  maskServerNames: boolean;
}

/** Message sent from the front-end to the back-end in Desktop mode. This is for opening external links etc */
export interface DesktopMessageDTO {
  /** The type of the DesktopMessageDTO */
  type: DesktopMessageType;
  /** The value of the DesktopMessageDTO */
  value: string;
}

/** The various types of Desktop messages that can be sent */
export enum DesktopMessageType {
  None = "None",
  ExternalLink = "ExternalLink",
  DesktopReady = "DesktopReady",
}

export interface DisplaySettingsDTO {
  allOverviewViewMode: PlexMediaType;
  movieViewMode: ViewMode;
  tvShowViewMode: ViewMode;
}

export enum DownloadActions {
  Details = "Details",
  Delete = "Delete",
  Start = "Start",
  Pause = "Pause",
  Stop = "Stop",
  Clear = "Clear",
  Restart = "Restart",
}

export interface DownloadJobUpdateDTO {
  id: DownloadTaskKey;
}

export interface DownloadManagerSettingsDTO {
  /** @format int32 */
  downloadSegments: number;
  keepCompletedInDownloadFolder: boolean;
}

export interface DownloadMediaDTO {
  keepCompletedInDownloadFolder: boolean;
  mediaIds: number[];
  /** @format int32 */
  plexLibraryId: number;
  /** @format int32 */
  plexServerId: number;
  qualities: PlexMediaQualityDTO[];
  type: PlexMediaType;
}

export interface DownloadPatchDTO {
  /** @format int64 */
  dataReceived: number;
  /** @format int64 */
  dataTotal: number;
  /** @format int64 */
  downloadSpeed: number;
  /** @format guid */
  id: string;
  /** @format guid */
  parentId: string;
  /** @format decimal */
  percentage: number;
  status: DownloadStatus;
  /** @format int32 */
  timeRemaining: number;
}

export interface DownloadPatchEntryMessagePackDTO {
  /** @format int64 */
  dataReceived: number;
  /** @format int64 */
  dataTotal: number;
  /** @format int64 */
  downloadSpeed: number;
  /** @format guid */
  id: string;
  /** @format guid */
  parentId: string;
  /** @format decimal */
  percentage: number;
  status: DownloadStatus;
  /** @format int32 */
  timeRemaining: number;
}

export interface DownloadPatchMessagePackDTO {
  deletedIds: string[];
  /** @format int64 */
  sequence: number;
  /** @format int32 */
  serverId: number;
  upserts: DownloadPatchEntryMessagePackDTO[];
}

export interface DownloadPreviewContainerDTO {
  expanded: Record<string, boolean>;
  previews: DownloadPreviewDTO[];
  /** @format int64 */
  totalSize: number;
}

export interface DownloadPreviewDTO {
  children: DownloadPreviewDTO[];
  key: string;
  qualities: PlexMediaQualityDTO[];
  /** @format int64 */
  size: number;
  title: string;
  type: PlexMediaType;
}

export interface DownloadProgressDTO {
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
  /** @format int32 */
  timeRemaining: number;
  title: string;
}

export interface DownloadProgressMessagePackDTO {
  children: DownloadProgressMessagePackDTO[];
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
  /** @format int32 */
  timeRemaining: number;
  title: string;
}

export enum DownloadStatus {
  Unknown = "Unknown",
  Error = "Error",
  Queued = "Queued",
  Downloading = "Downloading",
  DownloadFinished = "DownloadFinished",
  Moving = "Moving",
  MovePaused = "MovePaused",
  AutoMovePaused = "AutoMovePaused",
  MoveFinished = "MoveFinished",
  Completed = "Completed",
  Paused = "Paused",
  AutoPaused = "AutoPaused",
  Stopped = "Stopped",
  Deleted = "Deleted",
  ServerUnreachable = "ServerUnreachable",
  AuthError = "AuthError",
  StorageError = "StorageError",
  SourceUnavailable = "SourceUnavailable",
  DownloadClientError = "DownloadClientError",
  IntegrityError = "IntegrityError",
  MoveError = "MoveError",
  Restarting = "Restarting",
}

export interface DownloadTaskCreationReportDTO {
  /** @format int32 */
  episodes: number;
  /** @format int32 */
  movies: number;
  /** @format int32 */
  seasons: number;
  /** @format int32 */
  total: number;
  /** @format int32 */
  tvShows: number;
}

export interface DownloadTaskDTO {
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
  mediaType: PlexMediaType;
  /** @format guid */
  parentId: string;
  /** @format decimal */
  percentage: number;
  /** @format int32 */
  plexLibraryId: number;
  /** @format int32 */
  plexServerId: number;
  /** @format int64 */
  ratingKey: number;
  status: DownloadStatus;
  /** @format int32 */
  timeRemaining: number;
  title: string;
}

export interface DownloadTaskKey {
  /** @format guid */
  id: string;
  isValid: boolean;
  /** @format int32 */
  plexLibraryId: number;
  /** @format int32 */
  plexServerId: number;
  type: DownloadTaskType;
}

export interface DownloadTaskLogDTO {
  /** @format date-time */
  createdAt: string;
  /** @format int32 */
  id: number;
  logLevel: NotificationLevel;
  message: string;
  status: DownloadStatus;
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

export interface ErrorDTO {
  message: string;
  metadata: Record<string, any>;
  reasons: ErrorDTO[];
}

/** the dto used to send an error response to the client */
export interface ErrorResponse {
  /** the collection of errors for the current context */
  errors: Record<string, string[]>;
  /**
   * the message for the error response
   * @default "One or more errors occurred!"
   */
  message: string;
  /**
   * the http status code sent to the client. default is 400.
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
  hasAgreedToDisclaimer: boolean;
  hasBeenInvitedToDiscord: boolean;
  hideMediaFromOfflineServers: boolean;
  hideMediaFromOwnedServers: boolean;
  useLowQualityPosterImages: boolean;
}

export interface GeneratePlexTokenResponse {
  isUnAuthorized: boolean;
  needsVerificationCode: boolean;
  plexAuthToken: string;
}

/** Definition of an error */
export interface IError {
  /** Reasons of the error */
  reasons?: IError[] | null;
}

export interface InspectPlexServerJobUpdateDTO {
  plexServerIds: number[];
}

export interface IntegrationsSettingsDTO {
  downloadClientPassword: string;
  downloadClientUsername: string;
  radarr: RadarrSettingsDTO;
  reaparrApiKey: string;
  sonarr: SonarrSettingsDTO;
}

export enum JobStatus {
  Started = "Started",
  Completed = "Completed",
}

export interface JobStatusUpdateDTO {
  id: string;
  /** @format date-time */
  jobStartTime: string;
  jobType: JobTypes;
  jsonString: string;
  status: JobStatus;
}

export enum JobTypes {
  Unknown = "Unknown",
  CheckAllConnectionsStatusByPlexServerJob = "CheckAllConnectionsStatusByPlexServerJob",
  DownloadJob = "DownloadJob",
  MoveDownloadFileJob = "MoveDownloadFileJob",
  InspectPlexServerJob = "InspectPlexServerJob",
  LibrarySyncJob = "LibrarySyncJob",
  MetadataSyncJob = "MetadataSyncJob",
  CheckForUpdateJob = "CheckForUpdateJob",
  CheckPlexLibrariesForUpdatesJob = "CheckPlexLibrariesForUpdatesJob",
  LibraryComparisonJob = "LibraryComparisonJob",
}

export interface LanguageSettingsDTO {
  language: string;
}

export interface LibraryComparisonCompletedDTO {
  affectedLibraryIds: number[];
  /** @format date-time */
  completedAt: string;
  mediaType: PlexMediaType;
}

export interface LibrarySyncJobQueueDTO {
  /** @format date-time */
  completedAt?: string | null;
  /** @format date-time */
  createdAt: string;
  errorMessage?: string | null;
  isServerOffline: boolean;
  /** @format int32 */
  plexLibraryId: number;
  /** @format int32 */
  plexServerId: number;
  /** @format int32 */
  priority: number;
  /** @format date-time */
  startedAt?: string | null;
  status: LibrarySyncJobStatus;
}

export enum LibrarySyncJobStatus {
  Unknown = "Unknown",
  Queued = "Queued",
  Processing = "Processing",
  Completed = "Completed",
  Failed = "Failed",
  Cancelled = "Cancelled",
}

export interface LibrarySyncProgressDTO {
  errors: IError[];
  isComplete: boolean;
  items: LibrarySyncProgressItemDTO[];
  /** @format decimal */
  percentage: number;
  /** @format int32 */
  plexLibraryId: number;
  /** @format int32 */
  received: number;
  /** @format duration */
  timeRemaining: string;
  /** @format date-time */
  timeStamp: string;
  /** @format int32 */
  total: number;
}

export interface LibrarySyncProgressItemDTO {
  isComplete: boolean;
  mediaType: PlexMediaType;
  /** @format decimal */
  percentage: number;
  /** @format int32 */
  received: number;
  /** @format duration */
  timeRemaining: string;
  /** @format int32 */
  total: number;
}

export interface LiveLogEventDTO {
  exception?: string | null;
  level: LogSeverity;
  message: string;
  /** @format int64 */
  sequence: number;
  sourceContext?: string | null;
  /** @format date-time */
  timestamp: string;
}

export enum LogSeverity {
  None = "None",
  Verbose = "Verbose",
  Debug = "Debug",
  Information = "Information",
  Success = "Success",
  Warning = "Warning",
  Error = "Error",
  Fatal = "Fatal",
}

export interface MediaNavigationIndexDTO {
  /** @format int32 */
  index: number;
  label: string;
}

export interface MediaQueryFilterDTO {
  distinct?: boolean | null;
  filter?: string | null;
  /** @default false */
  filterOfflineMedia: boolean;
  /** @default false */
  filterOwnedMedia: boolean;
  groupBy?: string | null;
  having?: string | null;
  includeCount?: boolean | null;
  includes?: string | null;
  /** @default "None" */
  mediaType: PlexMediaType;
  mode?: string | null;
  /** @format int32 */
  page?: number | null;
  /** @format int32 */
  pageSize?: number | null;
  /** @format int32 */
  plexLibraryId: number;
  query?: string | null;
  select?: string | null;
  sort?: string | null;
}

export enum MessageTypes {
  LibraryProgress = "LibraryProgress",
  DownloadTaskUpdate = "DownloadTaskUpdate",
  ServerDownloadProgress = "ServerDownloadProgress",
  DownloadPatch = "DownloadPatch",
  ServerConnectionCheckStatusProgress = "ServerConnectionCheckStatusProgress",
  MoveDownloadFileProgress = "MoveDownloadFileProgress",
  Notification = "Notification",
  JobStatusUpdate = "JobStatusUpdate",
  RefreshNotification = "RefreshNotification",
  AppUpdateDownloadProgress = "AppUpdateDownloadProgress",
  LogEvent = "LogEvent",
  LibraryComparisonCompleted = "LibraryComparisonCompleted",
}

export interface MoveDownloadFileJobUpdateDTO {
  id: DownloadTaskKey;
}

export interface MovieLibraryComparisonDebugHitDTO {
  /** @format date-time */
  comparedAt: string;
  matchType: PlexMediaComparisonMatchType;
  /** @format int32 */
  ownedMediaId: number;
  ownedQuality: VideoQuality;
  ownedTitle: string;
  /** @format int32 */
  ownedYear: number;
  /** @format int32 */
  remoteMediaId: number;
  remoteQuality: VideoQuality;
  remoteTitle: string;
  /** @format int32 */
  remoteYear: number;
}

export interface MovieLibraryComparisonDebugResponseDTO {
  higherQuality: MovieLibraryComparisonDebugHitDTO[];
  matched: MovieLibraryComparisonDebugHitDTO[];
  /** @format int32 */
  missingCount: number;
  /** @format int32 */
  ownedLibraryId: number;
  /** @format int32 */
  remoteLibraryId: number;
}

export interface NetworkSettingsDTO {
  allowedProxyIps: string[];
  basePath: string;
  forwardedHostHeader: string;
  forwardedPathHeader: string;
  reverseProxyUrl: string;
  trustProxyHeaders: boolean;
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

export enum PlexAccessState {
  Unknown = "Unknown",
  Revoked = "Revoked",
  Updated = "Updated",
  Granted = "Granted",
}

export interface PlexAccountDTO {
  is2Fa: boolean;
  authenticationToken: string;
  clientId: string;
  customAuthenticationToken: string;
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
  validatedAt?: string | null;
  verificationCode: string;
}

export enum PlexConnectionTypes {
  Local = "Local",
  Public = "Public",
  PlexRelay = "PlexRelay",
  Unknown = "Unknown",
}

export interface PlexCountryDTO {
  /** @format int32 */
  id: number;
  name: string;
}

export interface PlexGenreDTO {
  /** @format int32 */
  id: number;
  name: string;
}

export interface PlexLibraryAccessCurrentStateDTO {
  libraries: PlexLibraryAccessCurrentStateLibraryDTO[];
  /** @format int32 */
  plexServerId?: number | null;
  plexServerName?: string | null;
}

export interface PlexLibraryAccessCurrentStateLibraryDTO {
  /** @format date-time */
  grantedAt: string;
  /** @format date-time */
  lastChangedAt: string;
  /** @format int32 */
  plexAccountId: number;
  plexAccountName: string;
  /** @format int32 */
  plexLibraryId?: number | null;
  plexLibraryName?: string | null;
  /** @format int32 */
  plexServerId?: number | null;
  plexServerName?: string | null;
}

export interface PlexLibraryAccessRapportDTO {
  /** @format int32 */
  plexLibraryId: number;
  plexLibraryName: string;
  /** @format int32 */
  plexServerId: number;
  state: PlexAccessState;
}

export interface PlexLibraryAccessTimelineDTO {
  currentState: PlexLibraryAccessCurrentStateDTO[];
  events: PlexLibraryAccessTimelineEventDTO[];
}

export interface PlexLibraryAccessTimelineEventDTO {
  /** @format date-time */
  createdAt: string;
  /** @format int32 */
  id: number;
  /** @format int32 */
  plexAccountId: number;
  plexAccountName: string;
  /** @format int32 */
  plexLibraryId?: number | null;
  plexLibraryName?: string | null;
  /** @format int32 */
  plexServerId?: number | null;
  plexServerName?: string | null;
  /** @format guid */
  refreshRunId: string;
  state: PlexAccessState;
}

export interface PlexLibraryDTO {
  /** @format int32 */
  count: number;
  /** @format date-time */
  createdAt?: string | null;
  defaultDestination?: FolderPathDTO | null;
  /** @format int32 */
  defaultDestinationId: number;
  /** @format int32 */
  episodeCount: number;
  /** @format int32 */
  id: number;
  isEnabled: boolean;
  key: string;
  /** @format int64 */
  mediaSize: number;
  outdated: boolean;
  /** @format int32 */
  plexServerId: number;
  /** @format date-time */
  scannedAt?: string | null;
  /** @format int32 */
  seasonCount: number;
  /** @format date-time */
  syncedAt?: string | null;
  title: string;
  type: PlexMediaType;
  /** @format date-time */
  updatedAt?: string | null;
  uuid: string;
}

export interface PlexMediaComparisonDetailsDTO {
  /** @format int32 */
  plexMediaId: number;
  rows: PlexMediaComparisonDetailsRowDTO[];
  state: PlexMediaComparisonState;
  type: PlexMediaType;
}

export interface PlexMediaComparisonDetailsRowDTO {
  children: PlexMediaComparisonDetailsRowDTO[];
  ownedQuality: VideoQuality;
  /** @format int32 */
  plexLibraryId: number;
  /** @format int32 */
  plexMediaId: number;
  /** @format int32 */
  plexServerId: number;
  remoteQuality: VideoQuality;
  state: PlexMediaComparisonState;
  title: string;
  type: PlexMediaType;
}

export enum PlexMediaComparisonMatchType {
  None = "None",
  TmdbGuid = "TmdbGuid",
  ImdbGuid = "ImdbGuid",
  TvdbGuid = "TvdbGuid",
  NormalizedTitleAndYear = "NormalizedTitleAndYear",
  NormalizedTitleYearAndDuration = "NormalizedTitleYearAndDuration",
  ParentAndChildNumbers = "ParentAndChildNumbers",
}

export enum PlexMediaComparisonState {
  NotCompared = "NotCompared",
  Owned = "Owned",
  Pending = "Pending",
  Missing = "Missing",
  HigherQuality = "HigherQuality",
  Partial = "Partial",
  PartialAndHigherQuality = "PartialAndHigherQuality",
  Unknown = "Unknown",
}

export interface PlexMediaDTO {
  /** @format date-time */
  addedAt: string;
  /** @format int32 */
  childCount: number;
  children: PlexMediaDTO[];
  /** @format int32 */
  comparisonId: number;
  contentRating?: string | null;
  /** @format int32 */
  duration: number;
  /** @format int32 */
  grandChildCount: number;
  hasArt: boolean;
  hasTheme: boolean;
  hasThumb: boolean;
  /** @format int32 */
  id: number;
  mediaData: PlexMediaDataDTO[];
  /** @format int64 */
  mediaSize: number;
  /** @format date-time */
  originallyAvailableAt?: string | null;
  /** @format int32 */
  plexApiMetaDataKey: number;
  /** @format int32 */
  plexApiRatingKey: number;
  /** @format int32 */
  plexLibraryId: number;
  /** @format int32 */
  plexServerId: number;
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
  audioCodec: string;
  /** @format int64 */
  duration: number;
  fileName: string;
  /** @format int32 */
  id: number;
  /** @format int32 */
  plexApiMediaId: number;
  /** @format int32 */
  plexApiPartId: number;
  /** @format int64 */
  size: number;
  videoCodec: string;
  videoResolution: VideoQuality;
}

export interface PlexMediaFilterMetadataDTO {
  countries: number[];
  genres: number[];
  qualities: number[];
  roles: number[];
}

export interface PlexMediaMetadataDTO {
  countries: PlexCountryDTO[];
  /** @format int32 */
  countryCount: number;
  /** @format int32 */
  genreCount: number;
  genres: PlexGenreDTO[];
  /** @format int32 */
  mediaCount: number;
  qualities: PlexQualityDTO[];
  /** @format int32 */
  qualityCount: number;
  /** @format int32 */
  roleCount: number;
  roles: PlexRoleDTO[];
}

export interface PlexMediaQualityDTO {
  /** @format int32 */
  dataId: number;
  mediaDataType: PlexMediaType;
  /** @format int32 */
  mediaId: number;
  quality: VideoQuality;
}

export interface PlexMediaSlimDTO {
  /** @format date-time */
  addedAt: string;
  /** @format int32 */
  childCount: number;
  /** @format int32 */
  comparisonId: number;
  /** @format int32 */
  duration: number;
  /** @format int32 */
  grandChildCount: number;
  hasThumb: boolean;
  /** @format int32 */
  id: number;
  /** @format int64 */
  mediaSize: number;
  /** @format int32 */
  plexApiMetaDataKey: number;
  /** @format int32 */
  plexApiRatingKey: number;
  /** @format int32 */
  plexLibraryId: number;
  /** @format int32 */
  plexServerId: number;
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
  countries: number[];
  /** @format int32 */
  episodeCount: number;
  genres: number[];
  /** @format int32 */
  mediaCount: number;
  mediaList: PlexMediaSlimDTO[];
  /** @format int64 */
  mediaSize: number;
  /** @format int32 */
  movieCount: number;
  navigationIndexes: MediaNavigationIndexDTO[];
  /** @format int32 */
  page: number;
  /** @format int32 */
  pageSize: number;
  qualities: number[];
  queryHash: string;
  roles: number[];
  /** @format int32 */
  seasonCount: number;
  /** @format int32 */
  totalCount: number;
  /** @format int32 */
  totalEpisodeCount: number;
  /** @format int64 */
  totalMediaSize: number;
  /** @format int32 */
  totalMovieCount: number;
  /** @format int32 */
  totalSeasonCount: number;
  /** @format int32 */
  totalTvShowCount: number;
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
  Artist = "Artist",
  Album = "Album",
  Song = "Song",
  PhotoAlbum = "PhotoAlbum",
  Photos = "Photos",
  OtherVideos = "OtherVideos",
  Games = "Games",
  Unknown = "Unknown",
}

export interface PlexQualityDTO {
  /** @format int32 */
  count: number;
  /** @format int32 */
  id: number;
  name: string;
  quality: VideoQuality;
}

export interface PlexRoleDTO {
  /** @format int32 */
  id: number;
  name: string;
}

export interface PlexServerAccessRapportDTO {
  isServerOffline: boolean;
  libraryAccess: PlexLibraryAccessRapportDTO[];
  /** @format int32 */
  plexServerId: number;
  plexServerName: string;
  state: PlexAccessState;
}

export interface PlexServerConnectionDTO {
  iPv4: boolean;
  iPv6: boolean;
  address: string;
  chosenConnection: boolean;
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
  type: PlexConnectionTypes;
  url: string;
}

export interface PlexServerDTO {
  /** @format date-time */
  createdAt: string;
  device: string;
  dnsRebindingProtection: boolean;
  httpsRequired: boolean;
  /** @format int32 */
  id: number;
  isDownloadsPausedByUser: boolean;
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
  allowStreamDownloader: boolean;
  /** @format int32 */
  downloadSpeedLimit: number;
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

export interface RadarrSettingsDTO {
  isConfigured: boolean;
  radarrApiKey: string;
  radarrBaseUrl: string;
}

export enum RefreshDataType {
  PlexAccount = "PlexAccount",
  PlexServer = "PlexServer",
  PlexLibrary = "PlexLibrary",
  PlexLibrarySyncStatus = "PlexLibrarySyncStatus",
  PlexServerConnection = "PlexServerConnection",
  DownloadTasks = "DownloadTasks",
  UpdateAvailable = "UpdateAvailable",
}

export interface RefreshPlexAccountAccessRapportDTO {
  access: PlexServerAccessRapportDTO[];
  /** @format int32 */
  plexAccountId: number;
  plexAccountName: string;
}

export interface ReleaseNoteDTO {
  isDevRelease: boolean;
  notes: string;
  /** @format date-time */
  releaseDate: string;
  version: string;
}

export interface ResultDTOOfAppCredentialsDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: AppCredentialsDTO | null;
}

export interface ResultDTOOfAppUpdateCheckDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: AppUpdateCheckDTO | null;
}

export interface ResultDTOOfBoolean {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value: boolean;
}

export interface ResultDTOOfCountResponseDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: CountResponseDTO | null;
}

export interface ResultDTOOfDownloadPreviewContainerDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: DownloadPreviewContainerDTO | null;
}

export interface ResultDTOOfDownloadTaskCreationReportDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: DownloadTaskCreationReportDTO | null;
}

export interface ResultDTOOfDownloadTaskDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: DownloadTaskDTO | null;
}

export interface ResultDTOOfFileSystemDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: FileSystemDTO | null;
}

export interface ResultDTOOfFolderPathDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: FolderPathDTO | null;
}

export interface ResultDTOOfGeneratePlexTokenResponse {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: GeneratePlexTokenResponse | null;
}

export interface ResultDTOOfInt32 {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  /** @format int32 */
  value: number;
}

export interface ResultDTOOfListOfDownloadTaskLogDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: DownloadTaskLogDTO[] | null;
}

export interface ResultDTOOfListOfFolderPathDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: FolderPathDTO[] | null;
}

export interface ResultDTOOfListOfJobStatusUpdateDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: JobStatusUpdateDTO[] | null;
}

export interface ResultDTOOfListOfLibrarySyncJobQueueDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: LibrarySyncJobQueueDTO[] | null;
}

export interface ResultDTOOfListOfLiveLogEventDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: LiveLogEventDTO[] | null;
}

export interface ResultDTOOfListOfNotificationDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: NotificationDTO[] | null;
}

export interface ResultDTOOfListOfPlexAccountDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexAccountDTO[] | null;
}

export interface ResultDTOOfListOfPlexLibraryDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexLibraryDTO[] | null;
}

export interface ResultDTOOfListOfPlexServerConnectionDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexServerConnectionDTO[] | null;
}

export interface ResultDTOOfListOfPlexServerDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexServerDTO[] | null;
}

export interface ResultDTOOfListOfPlexServerStatusDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexServerStatusDTO[] | null;
}

export interface ResultDTOOfListOfRefreshPlexAccountAccessRapportDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: RefreshPlexAccountAccessRapportDTO[] | null;
}

export interface ResultDTOOfListOfServerDownloadProgressDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: ServerDownloadProgressDTO[] | null;
}

export interface ResultDTOOfListOfString {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: string[] | null;
}

export interface ResultDTOOfMovieLibraryComparisonDebugResponseDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: MovieLibraryComparisonDebugResponseDTO | null;
}

export interface ResultDTOOfPlexAccountDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexAccountDTO | null;
}

export interface ResultDTOOfPlexLibraryAccessTimelineDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexLibraryAccessTimelineDTO | null;
}

export interface ResultDTOOfPlexLibraryDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexLibraryDTO | null;
}

export interface ResultDTOOfPlexMediaComparisonDetailsDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexMediaComparisonDetailsDTO | null;
}

export interface ResultDTOOfPlexMediaDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexMediaDTO | null;
}

export interface ResultDTOOfPlexMediaFilterMetadataDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexMediaFilterMetadataDTO | null;
}

export interface ResultDTOOfPlexMediaMetadataDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexMediaMetadataDTO | null;
}

export interface ResultDTOOfPlexMediaStatisticsDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexMediaStatisticsDTO | null;
}

export interface ResultDTOOfPlexServerConnectionDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexServerConnectionDTO | null;
}

export interface ResultDTOOfPlexServerDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexServerDTO | null;
}

export interface ResultDTOOfPlexServerStatusDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexServerStatusDTO | null;
}

export interface ResultDTOOfServerIdentityDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: ServerIdentityDTO | null;
}

export interface ResultDTOOfSettingsModelDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: SettingsModelDTO | null;
}

export interface ResultDTOOfString {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: string | null;
}

export interface ResultDTOOfTestConnectionToRadarrEndpointResponse {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: TestConnectionToRadarrEndpointResponse | null;
}

export interface ResultDTOOfTestConnectionToSonarrEndpointResponse {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: TestConnectionToSonarrEndpointResponse | null;
}

export interface ResultDTOOfTvShowLibraryComparisonDebugResponseDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: TvShowLibraryComparisonDebugResponseDTO | null;
}

export interface ResultDTOOfUserClaimsDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: UserClaimsDTO | null;
}

export interface ResultDTOOfValidatePlexCredentialsDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: ValidatePlexCredentialsDTO | null;
}

export interface ResultDTOOfValidatePlexTokenEndpointResponse {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: ValidatePlexTokenEndpointResponse | null;
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

export interface ServerDownloadProgressMessagePackDTO {
  /** @format int32 */
  downloadableTasksCount: number;
  downloads: DownloadProgressMessagePackDTO[];
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

export interface SetLibraryEnabledRequest {
  isEnabled: boolean;
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

export interface SetServerAliasRequest {
  serverAlias: string;
}

export interface SetServerEnabledRequest {
  isEnabled: boolean;
}

export interface SetServerOwnedRequest {
  isOwned: boolean;
}

export interface SettingsModelDTO {
  confirmationSettings: ConfirmationSettingsDTO;
  dateTimeSettings: DateTimeSettingsDTO;
  debugSettings: DebugSettingsDTO;
  displaySettings: DisplaySettingsDTO;
  downloadManagerSettings: DownloadManagerSettingsDTO;
  generalSettings: GeneralSettingsDTO;
  integrationsSettings: IntegrationsSettingsDTO;
  languageSettings: LanguageSettingsDTO;
  networkSettings: NetworkSettingsDTO;
  serverSettings: ServerSettingsDTO;
}

export interface SonarrSettingsDTO {
  isConfigured: boolean;
  sonarrApiKey: string;
  sonarrBaseUrl: string;
}

export interface SuccessDTO {
  message: string;
  metadata: Record<string, any>;
}

export enum TestConnectionStatus {
  Unknown = "Unknown",
  Success = "Success",
  UrlIsInvalid = "UrlIsInvalid",
  ConnectionFailed = "ConnectionFailed",
  InvalidApiKey = "InvalidApiKey",
}

export interface TestConnectionToRadarrEndpointResponse {
  result: TestConnectionStatus;
}

export interface TestConnectionToSonarrEndpointResponse {
  result: TestConnectionStatus;
}

export interface TvShowLibraryComparisonDebugEpisodeHitDTO {
  /** @format date-time */
  comparedAt: string;
  matchType: PlexMediaComparisonMatchType;
  /** @format int32 */
  ownedEpisodeNumber: number;
  /** @format int32 */
  ownedMediaId: number;
  ownedQuality: VideoQuality;
  /** @format int32 */
  ownedSeasonId: number;
  ownedTitle: string;
  /** @format int32 */
  ownedTvShowId: number;
  /** @format int32 */
  remoteEpisodeNumber: number;
  /** @format int32 */
  remoteMediaId: number;
  remoteQuality: VideoQuality;
  /** @format int32 */
  remoteSeasonId: number;
  remoteTitle: string;
  /** @format int32 */
  remoteTvShowId: number;
}

export interface TvShowLibraryComparisonDebugResponseDTO {
  episodes: TvShowLibraryComparisonDebugSectionDTOOfTvShowLibraryComparisonDebugEpisodeHitDTO;
  /** @format int32 */
  ownedLibraryId: number;
  /** @format int32 */
  remoteLibraryId: number;
  seasons: TvShowLibraryComparisonDebugSectionDTOOfTvShowLibraryComparisonDebugSeasonHitDTO;
  shows: TvShowLibraryComparisonDebugSectionDTOOfTvShowLibraryComparisonDebugShowHitDTO;
}

export interface TvShowLibraryComparisonDebugSeasonHitDTO {
  /** @format date-time */
  comparedAt: string;
  matchType: PlexMediaComparisonMatchType;
  /** @format int32 */
  ownedMediaId: number;
  ownedQuality: VideoQuality;
  /** @format int32 */
  ownedSeasonNumber: number;
  ownedTitle: string;
  /** @format int32 */
  ownedTvShowId: number;
  /** @format int32 */
  remoteMediaId: number;
  remoteQuality: VideoQuality;
  /** @format int32 */
  remoteSeasonNumber: number;
  remoteTitle: string;
  /** @format int32 */
  remoteTvShowId: number;
}

export interface TvShowLibraryComparisonDebugSectionDTOOfTvShowLibraryComparisonDebugEpisodeHitDTO {
  higherQuality: TvShowLibraryComparisonDebugEpisodeHitDTO[];
  matched: TvShowLibraryComparisonDebugEpisodeHitDTO[];
  /** @format int32 */
  missingCount: number;
}

export interface TvShowLibraryComparisonDebugSectionDTOOfTvShowLibraryComparisonDebugSeasonHitDTO {
  higherQuality: TvShowLibraryComparisonDebugSeasonHitDTO[];
  matched: TvShowLibraryComparisonDebugSeasonHitDTO[];
  /** @format int32 */
  missingCount: number;
}

export interface TvShowLibraryComparisonDebugSectionDTOOfTvShowLibraryComparisonDebugShowHitDTO {
  higherQuality: TvShowLibraryComparisonDebugShowHitDTO[];
  matched: TvShowLibraryComparisonDebugShowHitDTO[];
  /** @format int32 */
  missingCount: number;
}

export interface TvShowLibraryComparisonDebugShowHitDTO {
  /** @format date-time */
  comparedAt: string;
  matchType: PlexMediaComparisonMatchType;
  /** @format int32 */
  ownedMediaId: number;
  ownedQuality: VideoQuality;
  ownedTitle: string;
  /** @format int32 */
  ownedYear: number;
  /** @format int32 */
  remoteMediaId: number;
  remoteQuality: VideoQuality;
  remoteTitle: string;
  /** @format int32 */
  remoteYear: number;
}

/** @example {"username":"ReaparrRocks","password":"R€Aℙℙ@rr69"} */
export interface UpdateCredentialsEndpointRequest {
  /**
   * @minLength 8
   * @example "R€Aℙℙ@rr69"
   */
  password?: string | null;
  /**
   * @minLength 8
   * @example "ReaparrRocks"
   */
  username?: string | null;
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

export interface ValidatePlexCredentialsDTO {
  is2Fa: boolean;
  authenticationToken: string;
  clientId: string;
  email: string;
  isUnAuthorized: boolean;
  isValidated: boolean;
  password: string;
  /** @format int64 */
  plexId: number;
  title: string;
  username: string;
  uuid: string;
  /** @format date-time */
  validatedAt?: string | null;
}

export interface ValidatePlexCredentialsEndpointRequest {
  clientId: string;
  displayName: string;
  /** @minLength 5 */
  password: string;
  /** @minLength 5 */
  username: string;
  verificationCode: string;
}

export interface ValidatePlexServerConnectionEndpointRequest {
  /** @minLength 1 */
  url: string;
}

export interface ValidatePlexTokenEndpointRequest {
  /** @format int32 */
  plexAccountId: number;
  displayName: string;
  /** @minLength 5 */
  manualAuthenticationToken: string;
}

export interface ValidatePlexTokenEndpointResponse {
  is2Fa: boolean;
  clientId: string;
  customAuthenticationToken: string;
  email: string;
  isUnAuthorized: boolean;
  isValidated: boolean;
  /** @format int64 */
  plexId: number;
  title: string;
  username: string;
  uuid: string;
  /** @format date-time */
  validatedAt?: string | null;
}

export enum VideoQuality {
  Unknown = "Unknown",
  SubSD144P = "SubSD_144p",
  SubSDCIF = "SubSD_CIF",
  NHD = "nHD",
  SD = "SD",
  DVD = "DVD",
  HD = "HD",
  FullHD = "FullHD",
  QHD = "QHD",
  UHD_4K = "UHD_4K",
  UHD_8K = "UHD_8K",
  None = "None",
}

export enum ViewMode {
  Poster = "Poster",
  Table = "Table",
}
