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
  destinationPath: string;
  downloadPath: string;
  downloadUrl: string;
  children?: DownloadTaskDTO[] | null;
  actions: string[];
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

export interface DownloadClientUpdate {
  /** @format int32 */
  id: number;
  title: string;
  fullTitle: string;
  fileLocationUrl: string;
  fileName: string;
  status: DownloadStatus;

  /** @format decimal */
  percentage: number;

  /** @format int32 */
  downloadSpeed: number;

  /** @format int64 */
  dataReceived: number;

  /** @format int64 */
  dataTotal: number;
  downloadSpeedFormatted: string;
  mediaType: PlexMediaType;

  /** @format int64 */
  timeRemaining: number;

  /** @format int64 */
  bytesRemaining: number;
  workerProgresses: DownloadWorkerUpdate[];

  /** @format int32 */
  plexServerId: number;

  /** @format int32 */
  plexLibraryId: number;

  /** @format int32 */
  key: number;
  downloadTask: DownloadTask;
}

export interface DownloadWorkerUpdate {
  /** @format int32 */
  id?: number;
  downloadStatus?: DownloadStatus;

  /** @format int64 */
  dataReceived?: number;

  /** @format int64 */
  dataTotal?: number;

  /** @format int32 */
  downloadSpeed?: number;

  /** @format int32 */
  downloadSpeedAverage?: number;
  downloadSpeedFormatted?: string | null;

  /** @format int64 */
  timeRemaining?: number;

  /** @format int64 */
  timeElapsed?: number;

  /** @format int64 */
  bytesRemaining?: number;
  isCompleted?: boolean;

  /** @format decimal */
  percentage?: number;
}

export type DownloadTask = BaseEntity & {
  mediaType?: PlexMediaType;
  downloadStatus?: DownloadStatus;
  created?: string;
  key?: number;
  priority?: number;
  dataReceived?: number;
  serverToken?: string | null;
  metaData?: DownloadTaskMetaData | null;
  plexServer?: PlexServer | null;
  plexServerId?: number;
  plexLibrary?: PlexLibrary | null;
  plexLibraryId?: number;
  destinationFolder?: FolderPath | null;
  destinationFolderId?: number;
  downloadFolder?: FolderPath | null;
  downloadFolderId?: number;
  downloadWorkerTasks?: DownloadWorkerTask[] | null;
  dataTotal?: number;
  percentage?: number;
  releaseYear?: number;
  mediaParts?: number;
  title?: string | null;
  titlePath?: string | null;
  titleMovie?: string | null;
  titleTvShow?: string | null;
  titleTvShowSeason?: string | null;
  titleTvShowEpisode?: string | null;
  fileName?: string | null;
  fileLocationUrl?: string | null;
  downloadUrl?: string | null;
  downloadUri?: string | null;
  fileNameWithoutExtention?: string | null;
  downloadPath?: string | null;
  destinationPath?: string | null;
  mediaPath?: string | null;
};

export interface DownloadTaskMetaData {
  /** @format int32 */
  releaseYear?: number;
  movieTitle?: string | null;
  tvShowTitle?: string | null;
  tvShowSeasonTitle?: string | null;
  tvShowEpisodeTitle?: string | null;

  /** @format int32 */
  movieKey?: number;

  /** @format int32 */
  tvShowKey?: number;

  /** @format int32 */
  tvShowSeasonKey?: number;

  /** @format int32 */
  tvShowEpisodeKey?: number;
  mediaData?: PlexMediaData[] | null;
}

export interface PlexMediaData {
  mediaFormat?: string | null;

  /** @format int64 */
  duration?: number;
  videoResolution?: string | null;

  /** @format int32 */
  width?: number;

  /** @format int32 */
  height?: number;

  /** @format int32 */
  bitrate?: number;
  videoCodec?: string | null;
  videoFrameRate?: string | null;

  /** @format double */
  aspectRatio?: number;
  videoProfile?: string | null;
  audioProfile?: string | null;
  audioCodec?: string | null;

  /** @format int32 */
  audioChannels?: number;
  optimizedForStreaming?: boolean;
  protocol?: string | null;
  selected?: boolean;
  parts?: PlexMediaDataPart[] | null;
}

export interface PlexMediaDataPart {
  obfuscatedFilePath?: string | null;

  /** @format int32 */
  duration?: number;
  file?: string | null;

  /** @format int64 */
  size?: number;
  container?: string | null;
  videoProfile?: string | null;
  audioProfile?: string | null;
  hasThumbnail?: string | null;
  indexes?: string | null;
  hasChapterTextStream?: boolean | null;
}

export type PlexServer = BaseEntity & {
  name?: string | null;
  scheme?: string | null;
  address?: string | null;
  port?: number;
  version?: string | null;
  host?: string | null;
  localAddresses?: string | null;
  machineIdentifier?: string | null;
  createdAt?: string;
  updatedAt?: string;
  ownerId?: number;
  serverFixApplyDNSFix?: boolean;
  plexAccountServers?: PlexAccountServer[] | null;
  plexLibraries?: PlexLibrary[] | null;
  serverStatus?: PlexServerStatus[] | null;
  serverUrl?: string | null;
  libraryUrl?: string | null;
  accessToken?: string | null;
  hasDownloadTasks?: boolean;
  status?: PlexServerStatus | null;
};

export interface PlexAccountServer {
  /** @format int32 */
  plexAccountId?: number;
  plexAccount?: PlexAccount | null;

  /** @format int32 */
  plexServerId?: number;
  plexServer?: PlexServer | null;
  owned?: boolean;
  authToken?: string | null;

  /** @format date-time */
  authTokenCreationDate?: string;
}

export type PlexAccount = BaseEntity & {
  displayName?: string | null;
  username?: string | null;
  password?: string | null;
  isEnabled?: boolean;
  isValidated?: boolean;
  validatedAt?: string;
  plexId?: number;
  uuid?: string | null;
  email?: string | null;
  joinedAt?: string;
  title?: string | null;
  hasPassword?: boolean;
  authenticationToken?: string | null;
  isMain?: boolean;
  plexAccountServers?: PlexAccountServer[] | null;
  plexServers?: PlexServer[] | null;
};

export interface BaseEntity {
  /** @format int32 */
  id?: number;
}

export type PlexLibrary = BaseEntity & {
  key?: string | null;
  title?: string | null;
  type?: PlexMediaType;
  createdAt?: string;
  updatedAt?: string;
  scannedAt?: string;
  contentChangedAt?: string;
  checkedAt?: string;
  uuid?: string;
  libraryLocationId?: number;
  libraryLocationPath?: string | null;
  metaData?: PlexLibraryMetaData | null;
  plexServer?: PlexServer | null;
  plexServerId?: number;
  movies?: PlexMovie[] | null;
  tvShows?: PlexTvShow[] | null;
  plexAccountLibraries?: PlexAccountLibrary[] | null;
  downloadTasks?: DownloadTask[] | null;
  hasMedia?: boolean;
  mediaCount?: number;
  mediaSize?: number;
  movieCount?: number;
  tvShowCount?: number;
  seasonCount?: number;
  episodeCount?: number;
  serverUrl?: string | null;
  name?: string | null;
};

export interface PlexLibraryMetaData {
  /** @format int32 */
  tvShowCount?: number;

  /** @format int32 */
  tvShowSeasonCount?: number;

  /** @format int32 */
  tvShowEpisodeCount?: number;

  /** @format int32 */
  movieCount?: number;

  /** @format int64 */
  mediaSize?: number;
}

export type PlexMovie = PlexMedia & {
  plexMovieGenres?: PlexMovieGenre[] | null;
  plexMovieRoles?: PlexMovieRole[] | null;
  movieParts?: PlexMediaDataPart[] | null;
  movieData?: PlexMediaData[] | null;
  type?: PlexMediaType;
};

export type PlexMovieGenre = BaseEntity & {
  plexGenreId?: number;
  plexGenre?: PlexGenre | null;
  plexMoviesId?: number;
  plexMovie?: PlexMovie | null;
};

export type PlexGenre = BaseEntity & { tag?: string | null; plexMovies?: PlexMovieGenre[] | null };

export type PlexMovieRole = BaseEntity & {
  plexGenreId?: number;
  plexGenre?: PlexGenre | null;
  plexMoviesId?: number;
  plexMovie?: PlexMovie | null;
};

export type PlexMedia = BaseEntity & {
  key?: number;
  title?: string | null;
  year?: number;
  duration?: number;
  mediaSize?: number;
  metaDataKey?: number;
  hasThumb?: boolean;
  hasArt?: boolean;
  hasBanner?: boolean;
  hasTheme?: boolean;
  index?: number;
  studio?: string | null;
  summary?: string | null;
  contentRating?: string | null;
  rating?: number;
  childCount?: number;
  addedAt?: string;
  updatedAt?: string;
  originallyAvailableAt?: string | null;
  mediaData?: PlexMediaContainer | null;
  plexLibrary?: PlexLibrary | null;
  plexLibraryId?: number;
  plexServer?: PlexServer | null;
  plexServerId?: number;
  type?: PlexMediaType;
  metaDataUrl?: string | null;
  thumbUrl?: string | null;
  bannerUrl?: string | null;
  artUrl?: string | null;
  themeUrl?: string | null;
};

export interface PlexMediaContainer {
  mediaData?: PlexMediaData[] | null;
}

export type PlexTvShow = PlexMedia & {
  type?: PlexMediaType;
  plexTvShowGenres?: PlexTvShowGenre[] | null;
  plexTvShowRoles?: PlexTvShowRole[] | null;
  seasons?: PlexTvShowSeason[] | null;
};

export type PlexTvShowGenre = BaseEntity & {
  plexGenreId?: number;
  plexGenre?: PlexGenre | null;
  plexTvShowId?: number;
  plexTvShow?: PlexTvShow | null;
};

export type PlexTvShowRole = BaseEntity & {
  plexGenreId?: number;
  plexGenre?: PlexGenre | null;
  plexTvShowId?: number;
  plexTvShow?: PlexTvShow | null;
};

export type PlexTvShowSeason = PlexMedia & {
  parentKey?: number;
  tvShow?: PlexTvShow | null;
  tvShowId?: number;
  episodes?: PlexTvShowEpisode[] | null;
  type?: PlexMediaType;
};

export type PlexTvShowEpisode = PlexMedia & {
  parentKey?: number;
  tvShow?: PlexTvShow | null;
  tvShowId?: number;
  tvShowSeason?: PlexTvShowSeason | null;
  tvShowSeasonId?: number;
  episodeData?: PlexMediaData[] | null;
  type?: PlexMediaType;
};

export interface PlexAccountLibrary {
  /** @format int32 */
  plexAccountId?: number;
  plexAccount?: PlexAccount | null;

  /** @format int32 */
  plexLibraryId?: number;
  plexLibrary?: PlexLibrary | null;

  /** @format int32 */
  plexServerId?: number;
  plexServer?: PlexServer | null;
}

export type PlexServerStatus = BaseEntity & {
  statusCode?: number;
  isSuccessful?: boolean;
  statusMessage?: string | null;
  lastChecked?: string;
  plexServer?: PlexServer | null;
  plexServerId?: number;
};

export type FolderPath = BaseEntity & {
  type?: string | null;
  displayName?: string | null;
  directoryPath?: string | null;
  folderType?: FolderType;
};

/**
* 0 = Unknown
1 = DownloadFolder
2 = MovieFolder
3 = TvShowFolder
*/
export enum FolderType {
  Unknown = 0,
  DownloadFolder = 1,
  MovieFolder = 2,
  TvShowFolder = 3,
}

export type DownloadWorkerTask = BaseEntity & {
  fileName?: string | null;
  filePath?: string | null;
  partIndex?: number;
  startByte?: number;
  endByte?: number;
  downloadStatus?: DownloadStatus;
  bytesReceived?: number;
  tempDirectory?: string | null;
  elapsedTime?: number;
  downloadTask?: DownloadTask | null;
  downloadTaskId?: number;
  downloadWorkerTaskLogs?: DownloadWorkerLog[] | null;
  tempFileName?: string | null;
  tempFilePath?: string | null;
  url?: string | null;
  uri?: string | null;
  bytesRangeSize?: number;
  bytesRemaining?: number;
  currentByte?: number;
  elapsedTimeSpan?: string;
};

export type DownloadWorkerLog = BaseEntity & {
  createdAt?: string;
  logLevel?: LogEventLevel;
  message?: string | null;
  downloadTask?: DownloadWorkerTask | null;
  downloadWorkerTaskId?: number;
};

/**
* 0 = Verbose
1 = Debug
2 = Information
3 = Warning
4 = Error
5 = Fatal
*/
export enum LogEventLevel {
  Verbose = 0,
  Debug = 1,
  Information = 2,
  Warning = 3,
  Error = 4,
  Fatal = 5,
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
