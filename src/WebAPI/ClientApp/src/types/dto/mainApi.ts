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

export type ResultDTOOfListOfDownloadTaskDTO = ResultDTO & { value: DownloadTaskDTO[] };

export interface DownloadTaskDTO {
  /** @format int32 */
  id: number;
  title: string;
  fullTitle: string;
  status: DownloadStatus;
  fileLocationUrl: string;
  fileName: string;
  mediaType: PlexMediaType;

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

  /** @format int32 */
  priority: number;

  /** @format int32 */
  plexServerId: number;

  /** @format int32 */
  plexLibraryId: number;

  /** @format int64 */
  timeRemaining: number;
  destinationFilePath: string;
  downloadPath: string;
  downloadUrl: string;
  children?: DownloadTaskDTO[] | null;
  actions: string[];
}

export enum DownloadStatus {
  Unknown = "Unknown",
  Initialized = "Initialized",
  Queued = "Queued",
  Downloading = "Downloading",
  Paused = "Paused",
  Stopped = "Stopped",
  Deleted = "Deleted",
  Merging = "Merging",
  Moving = "Moving",
  Completed = "Completed",
  Error = "Error",
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
  Unknown = "Unknown",
}

export interface ResultDTO {
  isFailed?: boolean;
  isSuccess?: boolean;
  reasons?: Reason[] | null;
  errors?: Error[] | null;
  successes?: Success[] | null;
}

export interface Reason {
  message?: string | null;
  metadata?: Record<string, any>;
}

export type Error = Reason & { reasons?: Error[] | null };

export type Success = Reason & object;

export type ResultDTOOfListOfPlexServerDTO = ResultDTO & { value: PlexServerDTO[] };

export interface PlexServerDTO {
  /** @format int32 */
  id: number;
  name: string;
  address: string;

  /** @format int32 */
  port: number;
  version: string;
  scheme: string;
  host: string;
  localAddresses: string;
  serverUrl: string;
  machineIdentifier: string;

  /** @format date-time */
  createdAt: string;

  /** @format date-time */
  updatedAt: string;

  /** @format int32 */
  ownerId: number;
  plexLibraries: PlexLibraryDTO[];
  status: PlexServerStatusDTO;
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
  contentChangedAt: string;

  /** @format guid */
  uuid: string;

  /** @format int64 */
  mediaSize: number;

  /** @format int32 */
  libraryLocationId: number;
  libraryLocationPath: string;

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
  mediaData: PlexMediaDataDTO[] | null;
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
  Duration: number;
  File: string;

  /** @format int64 */
  Size: number;
  Container: string;
  VideoProfile: string;
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

export type ResultDTOOfBoolean = ResultDTO & { value: boolean };

export interface DownloadMediaDTO {
  mediaIds: number[];
  type: PlexMediaType;

  /** @format int32 */
  libraryId: number;

  /** @format int32 */
  plexAccountId: number;
}

export type ResultDTOOfListOfFolderPathDTO = ResultDTO & { value: FolderPathDTO[] };

export interface FolderPathDTO {
  /** @format int32 */
  id: number;
  type: string;
  displayName: string;
  directory: string;
  isValid: boolean;
}

export type ResultDTOOfFileSystemDTO = ResultDTO & { value: FileSystemDTO };

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
  lastModified: string | null;
}

export enum FileSystemEntityType {
  Parent = "Parent",
  Drive = "Drive",
  Folder = "Folder",
  File = "File",
}

export type ResultDTOOfListOfNotificationDTO = ResultDTO & { value: NotificationDTO[] };

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
  None = "none",
  Info = "info",
  Success = "success",
  Warning = "warning",
  Error = "error",
}

export type ResultDTOOfListOfPlexAccountDTO = ResultDTO & { value: PlexAccountDTO[] };

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
  email: string | null;

  /** @format date-time */
  joined_at: string;
  title: string;
  hasPassword: boolean;
  authToken: string;
  plexServers: PlexServerDTO[];
}

export type ResultDTOOfPlexAccountDTO = ResultDTO & { value: PlexAccountDTO };

export interface UpdatePlexAccountDTO {
  /** @format int32 */
  id: number;
  displayName: string;
  username: string;
  password: string;
  isEnabled: boolean;
  isMain: boolean;
}

export interface CreatePlexAccountDTO {
  displayName: string;
  username: string;
  password: string;
  isEnabled: boolean;
  isMain: boolean;
}

export interface CredentialsDTO {
  username?: string;
  password?: string;
}

export type ResultDTOOfPlexLibraryDTO = ResultDTO & { value: PlexLibraryDTO };

export type ResultDTOOfPlexServerDTO = ResultDTO & { value: PlexServerDTO };

export interface RefreshPlexLibraryDTO {
  /** @format int32 */
  plexAccountId?: number;

  /** @format int32 */
  plexLibraryId?: number;
}

export type ResultDTOOfPlexMediaDTO = ResultDTO & { value: PlexMediaDTO };

export type ResultDTOOfPlexServerStatusDTO = ResultDTO & { value: PlexServerStatusDTO };

export type ResultDTOOfSettingsModel = ResultDTO & { value: SettingsModel };

export type SettingsModel = BaseModel & {
  firstTimeSetup: boolean;
  accountSettings: AccountSettingsModel;
  advancedSettings: AdvancedSettingsModel;
  userInterfaceSettings: UserInterfaceSettingsModel;
};

export type AccountSettingsModel = BaseModel & { activeAccountId: number };

export type BaseModel = object;

export type AdvancedSettingsModel = BaseModel & { downloadManager: DownloadManagerModel };

export type DownloadManagerModel = BaseModel & { downloadSegments: number };

export type UserInterfaceSettingsModel = BaseModel & {
  confirmationSettings: ConfirmationSettingsModel;
  displaySettings: DisplaySettingsModel;
  dateTimeSettings: DateTimeModel;
};

export type ConfirmationSettingsModel = BaseModel & {
  askDownloadMovieConfirmation: boolean;
  askDownloadTvShowConfirmation: boolean;
  askDownloadSeasonConfirmation: boolean;
  askDownloadEpisodeConfirmation: boolean;
};

export type DisplaySettingsModel = BaseModel & { tvShowViewMode: ViewMode; movieViewMode: ViewMode };

export enum ViewMode {
  Table = "Table",
  Poster = "Poster",
  Overview = "Overview",
}

export type DateTimeModel = BaseModel & {
  shortDateFormat: string;
  longDateFormat: string;
  timeFormat: string;
  timeZone: string;
  showRelativeDates: boolean;
};

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
  isRefreshing: boolean;
  isComplete: boolean;
}

export interface PlexAccountRefreshProgress {
  /** @format int32 */
  plexAccountId: number;

  /** @format decimal */
  percentage: number;

  /** @format int32 */
  received: number;

  /** @format int32 */
  total: number;
  isRefreshing: boolean;
  isComplete: boolean;
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
