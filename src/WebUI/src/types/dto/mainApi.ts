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
  machineIdentifier: string;
  createdAt: string;
  updatedAt: string;
  owned: boolean;
  synced: boolean;
  ownerId: number;
  home: boolean;
  plexLibraries: PlexLibraryDTO[];
}

export interface PlexLibraryDTO {
  id?: number;
  key?: string;
  title?: string;
  type: PlexMediaType;
  updatedAt?: string;
  createdAt?: string;
  scannedAt?: string;
  contentChangedAt?: string;
  uuid?: string;
  libraryLocationId?: number;
  libraryLocationPath?: string;
  plexServerId?: number;
  count?: number;
  movies: PlexMovieDTO[];
  tvShows: PlexTvShowDTO[];
  downloadTasks: DownloadTaskDTO[];
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

export interface PlexMovieDTO {
  id: number;
  ratingKey?: number;
  key?: any;
  guid?: any;
  studio?: string | null;
  title?: string | null;
  contentRating?: string | null;
  summary?: string | null;
  index?: number;
  rating?: number;
  year?: number;
  thumb?: any;
  art?: any;
  banner?: any;
  duration?: number;
  originallyAvailableAt?: string;
  leafCount?: number;
  viewedLeafCount?: number;
  childCount?: number;
  addedAt?: string;
  updatedAt?: string;
  viewCount?: any;
  lastViewedAt?: any;
  theme?: any;
  plexLibraryId: number;
}

export interface PlexTvShowDTO {
  id: number;
  ratingKey?: number;
  key?: any;
  guid?: any;
  studio?: any;
  title?: any;
  contentRating?: any;
  summary?: any;
  index?: number;
  rating?: number;
  year?: number;
  thumb?: any;
  art?: any;
  banner?: any;
  duration?: number;
  originallyAvailableAt?: string;
  leafCount?: number;
  viewedLeafCount?: number;
  childCount?: number;
  addedAt?: string;
  updatedAt?: string;
  viewCount?: any;
  lastViewedAt?: any;
  theme?: any;
  plexLibraryId: number;
  seasons: PlexTvShowSeasonDTO[];
  type: PlexMediaType;
}

export interface PlexTvShowSeasonDTO {
  id: number;
  ratingKey?: number;
  key?: string | null;
  guid?: string | null;
  title?: string | null;
  summary?: string | null;
  index?: number;
  type: PlexMediaType;
  leafCount?: number;
  viewedLeafCount?: number;
  childCount?: number;
  addedAt?: string;
  updatedAt?: string;
  originallyAvailableAt?: string;
  tvShowId: number;
  episodes: PlexTvShowEpisodeDTO[];
  plexLibraryId: number;
}

export interface PlexTvShowEpisodeDTO {
  id: number;
  ratingKey?: number;
  key?: string | null;
  guid?: string | null;
  title?: string | null;
  summary?: string | null;
  index?: number;
  type: PlexMediaType;
  leafCount?: number;
  viewedLeafCount?: number;
  childCount?: number;
  addedAt?: string;
  updatedAt?: string;
  originallyAvailableAt?: string;
  tvShowSeasonId: number;
  plexLibraryId: number;
}

export interface DownloadTaskDTO {
  id: number;
  title: string;
  status: DownloadStatus;
  fileLocationUrl: string;
  FileName: string;
  titleTvShow: string;
  titleTvShowSeason: string;
  type: PlexMediaType;
  ratingKey: number;
  dataReceived: number;
  dataTotal: number;
  priority: number;
  plexServerId: number;
  plexLibraryId: number;
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

export type ResultDTOOfBoolean = ResultDTO & { value: boolean };

export interface DownloadMovieDTO {
  plexAccountId: number;
  plexMovieId: number;
}

export interface DownloadTvShowDTO {
  plexAccountId: number;
  plexMediaId: number;
  type: PlexMediaType;
}

export type ResultDTOOfListOfFolderPathDTO = ResultDTO & { value: FolderPathDTO[] };

export interface FolderPathDTO {
  id: number;
  type: string;
  displayName: string;
  directory: string;
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

/**
 * 0 = Parent
1 = Drive
2 = Folder
3 = File
 */
export type FileSystemEntityType = 0 | 1 | 2 | 3;

export type ResultDTOOfListOfPlexAccountDTO = ResultDTO & { value: PlexAccountDTO[] };

export interface PlexAccountDTO {
  id: number;
  displayName: string;
  username: string;
  password: string;
  isEnabled: boolean;
  isValidated: boolean;
  validatedAt: string;
  uuid: string;
  email: string | null;
  joined_at: string;
  title: string;
  hasPassword: boolean;
  authToken: string;
  forumId: number;
  plexServers: PlexServerDTO[];
}

export type ResultDTOOfPlexAccountDTO = ResultDTO & { value: PlexAccountDTO };

export interface UpdatePlexAccountDTO {
  id?: number;
  displayName?: string | null;
  username?: string | null;
  password?: string | null;
  isEnabled?: boolean;
}

export interface CreatePlexAccountDTO {
  displayName?: string;
  username?: string;
  password?: string;
  isEnabled?: boolean;
}

export interface CredentialsDTO {
  username?: string;
  password?: string;
}

export type ResultDTOOfPlexLibraryDTO = ResultDTO & { value: PlexLibraryDTO };

export interface RefreshPlexLibraryDTO {
  plexAccountId?: number;
  plexLibraryId?: number;
}

export type ResultDTOOfPlexServerDTO = ResultDTO & { value: PlexServerDTO };

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
