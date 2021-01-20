/* tslint:disable */
/* eslint-disable */
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
  id: number;
  title: string;
  status: DownloadStatus;
  fileLocationUrl: string;
  fileName: string;
  titleTvShow: string;
  titleTvShowSeason: string;
  mediaType: PlexMediaType;
  ratingKey: number;
  dataReceived: number;
  dataTotal: number;
  priority: number;
  plexServerId: number;
  plexLibraryId: number;
  destinationPath: string;
  downloadPath: string;
  downloadUrl: string;
}

export enum DownloadStatus {
  Unknown = "Unknown",
  Initialized = "Initialized",
  Starting = "Starting",
  Downloading = "Downloading",
  Pausing = "Pausing",
  Paused = "Paused",
  Stopping = "Stopping",
  Stopped = "Stopped",
  Queued = "Queued",
  Deleting = "Deleting",
  Deleted = "Deleted",
  Merging = "Merging",
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
  reasons?: Reason[] | null | null;
  errors?: Error[] | null | null;
  successes?: Success[] | null | null;
}

export interface Reason {
  message?: string | null;
  metadata?: Record<string, any>;
}

export type Error = Reason & { reasons?: Error[] | null | null };

export type Success = Reason & object;

export type ResultDTOOfListOfPlexServerDTO = ResultDTO & { value: PlexServerDTO[] };

export interface PlexServerDTO {
  id: number;
  name: string;
  address: string;
  port: number;
  version: string;
  scheme: string;
  host: string;
  localAddresses: string;
  serverUrl: string;
  machineIdentifier: string;
  createdAt: string;
  updatedAt: string;
  ownerId: number;
  plexLibraries: PlexLibraryDTO[];
  status: PlexServerStatusDTO;
}

export interface PlexLibraryDTO {
  id: number;
  key: string;
  title: string;
  type: PlexMediaType;
  updatedAt: string;
  createdAt: string;
  scannedAt: string;
  contentChangedAt: string;
  uuid: string;
  mediaSize: number;
  libraryLocationId: number;
  libraryLocationPath: string;
  plexServerId: number;
  count: number;
  seasonCount: number;
  episodeCount: number;
  movies: PlexMovieDTO[];
  tvShows: PlexTvShowDTO[];
  downloadTasks: DownloadTaskDTO[];
}

export type PlexMovieDTO = PlexMediaDTO & { plexMovieDatas: PlexMovieDataDTO[] };

export interface PlexMovieDataDTO {
  id: number;
  mediaFormat: string;
  duration: number;
  videoResolution: string;
  width: number;
  height: number;
  bitrate: number;
  videoCodec: string;
  videoFrameRate: string;
  aspectRatio: number;
  videoProfile: string;
  audioProfile: string;
  audioCodec: string;
  audioChannels: number;
  parts: PlexMovieDataPartDTO[];
}

export interface PlexMovieDataPartDTO {
  id: number;
  obfuscatedFilePath: string;
  Duration: number;
  File: string;
  Size: number;
  Container: string;
  VideoProfile: string;
}

export interface PlexMediaDTO {
  id: number;
  key: number;
  title: string;
  year: number;
  duration: number;
  mediaSize: number;
  hasThumb: boolean;
  hasArt: boolean;
  hasBanner: boolean;
  hasTheme: boolean;
  index: number;
  studio: string;
  summary: string;
  contentRating: string;
  rating: number;
  childCount: number;
  addedAt: string;
  updatedAt: string;
  originallyAvailableAt: string;
  plexLibraryId: number;
  plexServerId: number;
  type: PlexMediaType;
}

export type PlexTvShowDTO = PlexMediaDTO & { seasons: PlexTvShowSeasonDTO[] };

export type PlexTvShowSeasonDTO = PlexMediaDTO & { tvShowId: number; episodes: PlexTvShowEpisodeDTO[] };

export type PlexTvShowEpisodeDTO = PlexMediaDTO & { tvShowSeasonId: number };

export interface PlexServerStatusDTO {
  id: number;
  statusCode: number;
  isSuccessful: boolean;
  statusMessage: string;
  lastChecked: string;
  plexServerId: number;
}

export type ResultDTOOfBoolean = ResultDTO & { value: boolean };

export interface DownloadMediaDTO {
  mediaIds: number[];
  type: PlexMediaType;
  libraryId: number;
  plexAccountId: number;
}

export type ResultDTOOfListOfFolderPathDTO = ResultDTO & { value: FolderPathDTO[] };

export interface FolderPathDTO {
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
  size: number;
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
  id: number;
  level: NotificationLevel;
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
  id: number;
  displayName: string;
  username: string;
  password: string;
  isEnabled: boolean;
  isMain: boolean;
  isValidated: boolean;
  validatedAt: string;
  uuid: string;
  email: string | null;
  joined_at: string;
  title: string;
  hasPassword: boolean;
  authToken: string;
  plexServers: PlexServerDTO[];
}

export type ResultDTOOfPlexAccountDTO = ResultDTO & { value: PlexAccountDTO };

export interface UpdatePlexAccountDTO {
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
  plexAccountId?: number;
  plexLibraryId?: number;
}

export type ResultDTOOfPlexTvShowDTO = ResultDTO & { value: PlexTvShowDTO };

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

export type DownloadManagerModel = BaseModel & { downloadSegments?: number };

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

export interface DownloadProgress {
  id: number;
  percentage: number;
  downloadSpeed: number;
  dataReceived: number;
  dataTotal: number;
  downloadSpeedFormatted: string;
  timeRemaining: number;
  bytesRemaining: number;
  workerProgresses: IDownloadWorkerProgress[];
}

export interface IDownloadWorkerProgress {
  id?: number;
  dataReceived?: number;
  dataTotal?: number;
  downloadSpeed?: number;
  downloadSpeedFormatted?: string | null;
  timeRemaining?: number;
  bytesRemaining?: number;
  isCompleted?: boolean;
  percentage?: number;
  downloadSpeedAverage?: number;
}

export interface DownloadTaskCreationProgress {
  plexLibraryId: number;
  percentage: number;
  current: number;
  total: number;
  isComplete: boolean;
}

export interface LibraryProgress {
  id: number;
  percentage: number;
  received: number;
  total: number;
  isRefreshing: boolean;
  isComplete: boolean;
}

export interface PlexAccountRefreshProgress {
  plexAccountId: number;
  percentage: number;
  received: number;
  total: number;
  isRefreshing: boolean;
  isComplete: boolean;
}

export interface DownloadStatusChanged {
  id: number;
  status: DownloadStatus;
}

export interface FileMergeProgress {
  id: number;
  downloadTaskId: number;
  dataTransferred: number;
  dataTotal: number;
  percentage: number;
  transferSpeed: number;
  transferSpeedFormatted: string;
  timeRemaining: number;
  bytesRemaining: number;
}
