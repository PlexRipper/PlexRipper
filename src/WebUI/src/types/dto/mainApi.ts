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

export type ResultDTOOfIEnumerableOfDownloadTaskDTO = ResultDTO & { value: DownloadTaskDTO[] };

export interface DownloadTaskDTO {
  id: number;
  title: string;
  status: string;
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

export type ResultDTOOfIEnumerableOfFolderPathDTO = ResultDTO & { value: FolderPathDTO[] };

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

export type FileSystemEntityType = 0 | 1 | 2 | 3;

export type ResultDTOOfIEnumerableOfPlexAccountDTO = ResultDTO & { value: PlexAccountDTO[] };

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

export interface PlexServerDTO {
  id?: number;
  accessToken?: string;
  name?: string;
  address?: string;
  port?: number;
  version?: string;
  scheme?: string;
  host?: string;
  localAddresses?: string;
  machineIdentifier?: string;
  createdAt?: string;
  updatedAt?: string;
  owned?: boolean;
  synced?: boolean;
  ownerId?: number;
  home?: boolean;
  plexLibraries?: PlexLibraryDTO[];
}

export interface PlexLibraryDTO {
  id?: number;
  key?: string | null;
  title?: string | null;
  type?: string | null;
  updatedAt?: string;
  createdAt?: string;
  scannedAt?: string;
  contentChangedAt?: string;
  uuid?: string;
  libraryLocationId?: number;
  libraryLocationPath?: string | null;
  plexServerId?: number;
  count?: number;
  movies?: PlexMovieDTO[] | null | null;
  tvShows?: PlexTvShowDTO[] | null | null;
}

export type PlexMovieDTO = PlexMediaDTO & object;

export interface PlexMediaDTO {
  id?: number;
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
  plexLibraryId?: number;
}

export type PlexTvShowDTO = PlexMediaDTO & { seasons?: PlexTvShowSeasonDTO[] | null | null };

export interface PlexTvShowSeasonDTO {
  id?: number;
  ratingKey?: number;
  key?: string | null;
  guid?: string | null;
  title?: string | null;
  summary?: string | null;
  index?: number;
  type?: string | null;
  leafCount?: number;
  viewedLeafCount?: number;
  childCount?: number;
  addedAt?: string;
  updatedAt?: string;
  originallyAvailableAt?: string;
  tvShowId?: number;
  episodes?: PlexTvShowEpisodeDTO[] | null | null;
}

export interface PlexTvShowEpisodeDTO {
  id?: number;
  ratingKey?: number;
  key?: string | null;
  guid?: string | null;
  title?: string | null;
  summary?: string | null;
  index?: number;
  type?: string | null;
  leafCount?: number;
  viewedLeafCount?: number;
  childCount?: number;
  addedAt?: string;
  updatedAt?: string;
  originallyAvailableAt?: string;
  tvShowSeasonId?: number;
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

export type RequestParams = Omit<RequestInit, "body" | "method"> & {
  secure?: boolean;
};

export type RequestQueryParamsType = Record<string | number, any>;

type ApiConfig<SecurityDataType> = {
  baseUrl?: string;
  baseApiParams?: RequestParams;
  securityWorker?: (securityData: SecurityDataType) => RequestParams;
};

const enum BodyType {
  Json,
}

