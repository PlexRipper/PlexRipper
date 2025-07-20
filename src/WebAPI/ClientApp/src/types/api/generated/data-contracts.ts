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

export interface AppCredentialsDTO {
  isDefaultCredentials: boolean;
  password: string;
  userName: string;
}

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
  maskLibraryNames: boolean;
  maskServerNames: boolean;
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

export interface DownloadTaskKey {
  /** @format guid */
  id: string;
  isDownloadable: boolean;
  isValid: boolean;
  /** @format int32 */
  plexLibraryId: number;
  /** @format int32 */
  plexServerId: number;
  type: DownloadTaskType;
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

export interface FileMergeJobUpdateDTO {
  id: DownloadTaskKey;
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

export interface IError {
  reasons?: IError[] | null;
}

export interface InspectPlexServerJobUpdateDTO {
  plexServerIds: number[];
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
  FileMergeJob = "FileMergeJob",
  SyncServerMediaJob = "SyncServerMediaJob",
  InspectPlexServerJob = "InspectPlexServerJob",
}

export interface LanguageSettingsDTO {
  language: string;
}

export interface LibraryMediaItemPartDTO {
  accessible?: boolean | null;
  audioProfile: string;
  container: string;
  /** @format int32 */
  duration: number;
  exists?: boolean | null;
  file: string;
  /** @format int64 */
  id: number;
  indexes?: string | null;
  key: string;
  /** @format int64 */
  size: number;
  stream: LibraryMediaItemStreamDTO[];
  videoProfile: string;
}

export interface LibraryMediaItemStreamDTO {
  audioChannelLayout?: string | null;
  /** @format int32 */
  bitDepth?: number | null;
  /** @format int32 */
  bitrate: number;
  canAutoSync?: boolean | null;
  /** @format int32 */
  channels?: number | null;
  chromaLocation?: string | null;
  chromaSubsampling?: string | null;
  codec: string;
  /** @format int32 */
  codedHeight?: number | null;
  /** @format int32 */
  codedWidth?: number | null;
  colorPrimaries?: string | null;
  colorRange?: string | null;
  colorSpace?: string | null;
  colorTrc?: string | null;
  default?: boolean | null;
  displayTitle: string;
  /** @format int32 */
  doviLevel?: number | null;
  doviPresent?: boolean | null;
  /** @format int32 */
  doviProfile?: number | null;
  doviVersion?: string | null;
  /** @format int32 */
  doviblCompatID?: number | null;
  doviblPresent?: boolean | null;
  dovielPresent?: boolean | null;
  dovirpuPresent?: boolean | null;
  dub?: boolean | null;
  extendedDisplayTitle: string;
  forced?: boolean | null;
  /** @format float */
  frameRate?: number | null;
  hasScalingMatrix?: boolean | null;
  hearingImpaired?: boolean | null;
  /** @format int32 */
  height?: number | null;
  /** @format int64 */
  id: number;
  /** @format int32 */
  index?: number | null;
  language: string;
  languageCode: string;
  languageTag: string;
  /** @format int32 */
  level?: number | null;
  original?: boolean | null;
  profile?: string | null;
  /** @format int32 */
  refFrames?: number | null;
  /** @format int32 */
  samplingRate?: number | null;
  scanType?: string | null;
  selected?: boolean | null;
  streamType: StreamType;
  title?: string | null;
  /** @format int32 */
  width?: number | null;
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

export enum PlexAccessState {
  Unknown = "Unknown",
  Revoked = "Revoked",
  Updated = "Updated",
  Granted = "Granted",
}

export interface PlexAccountDTO {
  is2Fa: boolean;
  apiAuthenticationToken: string;
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

export interface PlexLibraryAccessRapportDTO {
  /** @format int32 */
  plexLibraryId: number;
  plexLibraryName: string;
  /** @format int32 */
  plexServerId: number;
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
  parts: LibraryMediaItemPartDTO[];
  videoCodec: string;
  videoFrameRate: string;
  videoProfile: string;
  videoResolution: string;
  /** @format int32 */
  width: number;
}

export interface PlexMediaMetadataDTO {
  countries: PlexCountryDTO[];
  /** @format int32 */
  countryCount: number;
  /** @format int32 */
  genreCount: number;
  genres: PlexGenreDTO[];
  /** @format int32 */
  roleCount: number;
  roles: PlexRoleDTO[];
}

export interface PlexMediaQualityDTO {
  /** @format int32 */
  id: number;
  quality: VideoQuality;
  type: PlexMediaType;
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
  Artist = "Artist",
  Album = "Album",
  Song = "Song",
  PhotoAlbum = "PhotoAlbum",
  Photos = "Photos",
  OtherVideos = "OtherVideos",
  Games = "Games",
  Unknown = "Unknown",
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

export enum RefreshDataType {
  PlexAccount = "PlexAccount",
  PlexServer = "PlexServer",
  PlexLibrary = "PlexLibrary",
  PlexServerConnection = "PlexServerConnection",
}

export interface RefreshPlexAccountAccessRapportDTO {
  access: PlexServerAccessRapportDTO[];
  /** @format int32 */
  plexAccountId: number;
  plexAccountName: string;
}

export interface ResultDTOOfAppCredentialsDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: AppCredentialsDTO | null;
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

export interface ResultDTOOfListOfDownloadPreviewDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: DownloadPreviewDTO[] | null;
}

export interface ResultDTOOfListOfDownloadWorkerLogDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: DownloadWorkerLogDTO[] | null;
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

export interface ResultDTOOfListOfPlexMediaSlimDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexMediaSlimDTO[] | null;
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

export interface ResultDTOOfPlexAccountDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexAccountDTO | null;
}

export interface ResultDTOOfPlexLibraryDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexLibraryDTO | null;
}

export interface ResultDTOOfPlexMediaDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: PlexMediaDTO | null;
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

export interface ResultDTOOfUserClaimsDTO {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: UserClaimsDTO | null;
}

export interface ResultDTOOfValidatePlexAccountResponse {
  errors: ErrorDTO[];
  isSuccess: boolean;
  /** @format int32 */
  statusCode: number;
  successes: SuccessDTO[];
  value?: ValidatePlexAccountResponse | null;
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

export enum StreamType {
  Unknown = "Unknown",
  Video = "Video",
  Audio = "Audio",
  Subtitle = "Subtitle",
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
  libraryProgresses: LibraryProgress[];
  /** @format decimal */
  percentage: number;
  /** @format int32 */
  serverId: number;
}

/** @example {"username":"PlexRipperRocks","password":"Pl€XR!ℙℙ€R69"} */
export interface UpdateCredentialsEndpointRequest {
  /** @minLength 8 */
  password?: string | null;
  /** @minLength 8 */
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

export interface ValidatePlexAccountResponse {
  isUnAuthorized: boolean;
  plexAccountDTO: PlexAccountDTO;
}

export interface ValidatePlexServerConnectionEndpointRequest {
  /** @minLength 1 */
  url: string;
}

export enum VideoQuality {
  Unknown = "Unknown",
  SD = "SD",
  DVD = "DVD",
  HD = "HD",
  FullHD = "FullHD",
  QHD = "QHD",
  UHD4K = "UHD_4K",
  UHD8K = "UHD_8K",
}

export enum ViewMode {
  Poster = "Poster",
  Table = "Table",
}
