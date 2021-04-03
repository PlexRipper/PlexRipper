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
  fullTitle: string;
  status: DownloadStatus;
  fileLocationUrl: string;
  fileName: string;
  mediaType: PlexMediaType;
  key: number;
  downloadSpeed: number;
  dataReceived: number;
  dataTotal: number;
  percentage: number;
  priority: number;
  plexServerId: number;
  plexLibraryId: number;
  timeRemaining: number;
  destinationPath: string;
  downloadPath: string;
  downloadUrl: string;
  children: DownloadTaskDTO[] | null;
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
  movies: PlexMediaDTO[];
  tvShows: PlexMediaDTO[];
  downloadTasks: DownloadTaskDTO[];
}

export interface PlexMediaDTO {
  id: number;
  key: number;
  treeKeyId: string;
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
  tvShowId: number;
  tvShowSeasonId: number;
  plexLibraryId: number;
  plexServerId: number;
  type: PlexMediaType;
  mediaData: PlexMediaDataDTO[] | null;
  children: PlexMediaDTO[];
}

export interface PlexMediaDataDTO {
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
  parts: PlexMediaDataPartDTO[];
}

export interface PlexMediaDataPartDTO {
  obfuscatedFilePath: string;
  Duration: number;
  File: string;
  Size: number;
  Container: string;
  VideoProfile: string;
}

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
  id: number;
  title: string;
  fullTitle: string;
  fileLocationUrl: string;
  fileName: string;
  status: DownloadStatus;
  percentage: number;
  downloadSpeed: number;
  dataReceived: number;
  dataTotal: number;
  downloadSpeedFormatted: string;
  timeRemaining: number;
  bytesRemaining: number;
  workerProgresses: DownloadWorkerUpdate[];
  plexServerId: number;
  plexLibraryId: number;
  key: number;
  downloadTask: DownloadTask;
}

export interface DownloadWorkerUpdate {
  id?: number;
  downloadStatus?: DownloadStatus;
  dataReceived?: number;
  dataTotal?: number;
  downloadSpeed?: number;
  downloadSpeedAverage?: number;
  downloadSpeedFormatted?: string | null;
  timeRemaining?: number;
  bytesRemaining?: number;
  isCompleted?: boolean;
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
  releaseYear?: number;
  movieTitle?: string | null;
  tvShowTitle?: string | null;
  tvShowSeasonTitle?: string | null;
  tvShowEpisodeTitle?: string | null;
  movieKey?: number;
  tvShowKey?: number;
  tvShowSeasonKey?: number;
  tvShowEpisodeKey?: number;
  mediaData?: PlexMediaData[] | null;
}

export interface PlexMediaData {
  mediaFormat?: string | null;
  duration?: number;
  videoResolution?: string | null;
  width?: number;
  height?: number;
  bitrate?: number;
  videoCodec?: string | null;
  videoFrameRate?: string | null;
  aspectRatio?: number;
  videoProfile?: string | null;
  audioProfile?: string | null;
  audioCodec?: string | null;
  audioChannels?: number;
  optimizedForStreaming?: boolean;
  protocol?: string | null;
  selected?: boolean;
  parts?: PlexMediaDataPart[] | null;
}

export interface PlexMediaDataPart {
  obfuscatedFilePath?: string | null;
  duration?: number;
  file?: string | null;
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
  plexAccountId?: number;
  plexAccount?: PlexAccount | null;
  plexServerId?: number;
  plexServer?: PlexServer | null;
  owned?: boolean;
  authToken?: string | null;
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
  tvShowCount?: number;
  tvShowSeasonCount?: number;
  tvShowEpisodeCount?: number;
  movieCount?: number;
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
  plexAccountId?: number;
  plexAccount?: PlexAccount | null;
  plexLibraryId?: number;
  plexLibrary?: PlexLibrary | null;
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
  url?: string | null;
  tempDirectory?: string | null;
  elapsedTime?: number;
  downloadTask?: DownloadTask | null;
  downloadTaskId?: number;
  downloadWorkerTaskLogs?: DownloadWorkerLog[] | null;
  tempFileName?: string | null;
  tempFilePath?: string | null;
  uri?: string | null;
  bytesRangeSize?: number;
  bytesRemaining?: number;
  currentByte?: number;
  isResumed?: boolean;
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
  plexServerId: number;
  plexLibraryId: number;
}
