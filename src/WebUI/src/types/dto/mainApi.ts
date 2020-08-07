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

export type ResultDTOOfIEnumerableOfDownloadTaskDTO = ResultDTO & { value?: DownloadTaskDTO[] | null | null };

export interface DownloadTaskDTO {
  id?: number;
  title?: string | null;
  status?: string | null;
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

export type ResultDTOOfBoolean = ResultDTO & { value?: boolean };

export interface DownloadMovieDTO {
  plexAccountId?: number;
  plexMovieId?: number;
}

export type ResultDTOOfIEnumerableOfFolderPath = ResultDTO & { value?: FolderPath[] | null | null };

export type FolderPath = BaseEntity & {
  type?: string | null;
  displayName?: string | null;
  directory?: string | null;
  folderType?: FolderType;
};

export type FolderType = 0 | 1 | 2 | 3;

export interface BaseEntity {
  id?: number;
}

export type ResultDTOOfFileSystemResult = ResultDTO & { value?: FileSystemResult | null };

export interface FileSystemResult {
  parent?: string | null;
  directories?: FileSystemModel[] | null | null;
  files?: FileSystemModel[] | null | null;
}

export interface FileSystemModel {
  type?: FileSystemEntityType;
  name?: string | null;
  path?: string | null;
  extension?: string | null;
  size?: number;
  lastModified?: string | null;
}

export type FileSystemEntityType = 0 | 1 | 2 | 3;

export type ResultDTOOfIEnumerableOfPlexAccountDTO = ResultDTO & { value?: PlexAccountDTO[] | null | null };

export interface PlexAccountDTO {
  id?: number;
  displayName?: string;
  username?: string;
  password?: string;
  isEnabled?: boolean;
  isValidated?: boolean;
  validatedAt?: string;
  uuid?: string;
  email?: string;
  joined_at?: string;
  title?: string;
  hasPassword?: boolean;
  authToken?: string;
  authentication_token?: string;
  forumId?: any;
  plexServers: PlexServerDTO[];
}

export interface PlexServerDTO {
  id?: number;
  accessToken?: string | null;
  name?: string | null;
  address?: string | null;
  port?: number;
  version?: string | null;
  scheme?: string | null;
  host?: string | null;
  localAddresses?: string | null;
  machineIdentifier?: string | null;
  createdAt?: string;
  updatedAt?: string;
  owned?: boolean;
  synced?: boolean;
  ownerId?: number;
  home?: boolean;
  plexLibraries?: PlexLibraryDTO[] | null | null;
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

export type ResultDTOOfPlexAccountDTO = ResultDTO & { value?: PlexAccountDTO | null };

export interface UpdatePlexAccountDTO {
  id?: number;
  displayName?: string | null;
  username?: string | null;
  password?: string | null;
  isEnabled?: boolean;
}

export interface CreatePlexAccountDTO {
  displayName?: string | null;
  username?: string | null;
  password?: string | null;
  isEnabled?: boolean;
}

export interface CredentialsDTO {
  username?: string | null;
  password?: string | null;
}

export type ResultDTOOfPlexLibraryDTO = ResultDTO & { value?: PlexLibraryDTO | null };

export interface RefreshPlexLibraryDTO {
  plexAccountId?: number;
  plexLibraryId?: number;
}

export type ResultDTOOfPlexServerDTO = ResultDTO & { value?: PlexServerDTO | null };

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

class HttpClient<SecurityDataType> {
  public baseUrl: string = "http://localhost:5000";
  private securityData: SecurityDataType = null as any;
  private securityWorker: ApiConfig<SecurityDataType>["securityWorker"] = (() => {}) as any;

  private baseApiParams: RequestParams = {
    credentials: "same-origin",
    headers: {
      "Content-Type": "application/json",
    },
    redirect: "follow",
    referrerPolicy: "no-referrer",
  };

  constructor({ baseUrl, baseApiParams, securityWorker }: ApiConfig<SecurityDataType> = {}) {
    this.baseUrl = baseUrl || this.baseUrl;
    this.baseApiParams = baseApiParams || this.baseApiParams;
    this.securityWorker = securityWorker || this.securityWorker;
  }

  public setSecurityData = (data: SecurityDataType) => {
    this.securityData = data;
  };

  private addQueryParam(query: RequestQueryParamsType, key: string) {
    return (
      encodeURIComponent(key) + "=" + encodeURIComponent(Array.isArray(query[key]) ? query[key].join(",") : query[key])
    );
  }

  protected addQueryParams(rawQuery?: RequestQueryParamsType): string {
    const query = rawQuery || {};
    const keys = Object.keys(query).filter((key) => "undefined" !== typeof query[key]);
    return keys.length
      ? `?${keys
          .map((key) =>
            typeof query[key] === "object" && !Array.isArray(query[key])
              ? this.addQueryParams(query[key] as object).substring(1)
              : this.addQueryParam(query, key),
          )
          .join("&")}`
      : "";
  }

  private bodyFormatters: Record<BodyType, (input: any) => any> = {
    [BodyType.Json]: JSON.stringify,
  };

  private mergeRequestOptions(params: RequestParams, securityParams?: RequestParams): RequestParams {
    return {
      ...this.baseApiParams,
      ...params,
      ...(securityParams || {}),
      headers: {
        ...(this.baseApiParams.headers || {}),
        ...(params.headers || {}),
        ...((securityParams && securityParams.headers) || {}),
      },
    };
  }

  private safeParseResponse = <T = any, E = any>(response: Response): Promise<T> =>
    response
      .json()
      .then((data) => data)
      .catch((e) => response.text);

  public request = <T = any, E = any>(
    path: string,
    method: string,
    { secure, ...params }: RequestParams = {},
    body?: any,
    bodyType?: BodyType,
    secureByDefault?: boolean,
  ): Promise<T> =>
    fetch(`${this.baseUrl}${path}`, {
      // @ts-ignore
      ...this.mergeRequestOptions(params, (secureByDefault || secure) && this.securityWorker(this.securityData)),
      method,
      body: body ? this.bodyFormatters[bodyType || BodyType.Json](body) : null,
    }).then(async (response) => {
      const data = await this.safeParseResponse<T, E>(response);
      if (!response.ok) throw data;
      return data;
    });
}

/**
 * @title PlexRipper API
 * @version 1.0.0
 * @baseUrl http://localhost:5000
 */
export class Api<SecurityDataType = any> extends HttpClient<SecurityDataType> {
  api = {
    /**
     * @tags Download
     * @name Download_Get
     * @request GET:/api/Download
     */
    downloadGet: (params?: RequestParams) =>
      this.request<ResultDTOOfIEnumerableOfDownloadTaskDTO, ResultDTO>(`/api/Download`, "GET", params),

    /**
     * @tags Download
     * @name Download_Post
     * @request POST:/api/Download/movie
     */
    downloadPost: (data: DownloadMovieDTO, params?: RequestParams) =>
      this.request<ResultDTOOfBoolean, ResultDTO>(`/api/Download/movie`, "POST", params, data),

    /**
     * @tags Download
     * @name Download_Delete
     * @request DELETE:/api/Download/{downloadTaskId}
     */
    downloadDelete: (downloadTaskId: number, params?: RequestParams) =>
      this.request<ResultDTOOfBoolean, ResultDTO>(`/api/Download/${downloadTaskId}`, "DELETE", params),

    /**
     * @tags FolderPath
     * @name FolderPath_Get
     * @request GET:/api/FolderPath
     */
    folderPathGet: (params?: RequestParams) =>
      this.request<ResultDTOOfIEnumerableOfFolderPath, any>(`/api/FolderPath`, "GET", params),

    /**
     * @tags FolderPath
     * @name FolderPath_Put
     * @request PUT:/api/FolderPath
     */
    folderPathPut: (data: FolderPath, params?: RequestParams) =>
      this.request<ResultDTOOfIEnumerableOfFolderPath, ResultDTO>(`/api/FolderPath`, "PUT", params, data),

    /**
     * @tags FolderPath
     * @name FolderPath_Get2
     * @request GET:/api/FolderPath/directory
     */
    folderPathGet2: (query?: { path?: string | null }, params?: RequestParams) =>
      this.request<ResultDTOOfFileSystemResult, any>(
        `/api/FolderPath/directory${this.addQueryParams(query)}`,
        "GET",
        params,
      ),

    /**
     * @tags PlexAccount
     * @name PlexAccount_GetAll
     * @request GET:/api/PlexAccount
     */
    plexAccountGetAll: (query?: { enabledOnly?: boolean }, params?: RequestParams) =>
      this.request<ResultDTOOfIEnumerableOfPlexAccountDTO, ResultDTO>(
        `/api/PlexAccount${this.addQueryParams(query)}`,
        "GET",
        params,
      ),

    /**
     * @tags PlexAccount
     * @name PlexAccount_Post
     * @request POST:/api/PlexAccount
     */
    plexAccountPost: (data: CreatePlexAccountDTO, params?: RequestParams) =>
      this.request<ResultDTOOfPlexAccountDTO, ResultDTO>(`/api/PlexAccount`, "POST", params, data),

    /**
     * @tags PlexAccount
     * @name PlexAccount_Get
     * @request GET:/api/PlexAccount/{id}
     */
    plexAccountGet: (id: number, params?: RequestParams) =>
      this.request<ResultDTOOfPlexAccountDTO, ResultDTO>(`/api/PlexAccount/${id}`, "GET", params),

    /**
     * @tags PlexAccount
     * @name PlexAccount_Put
     * @request PUT:/api/PlexAccount/{id}
     */
    plexAccountPut: (id: number, data: UpdatePlexAccountDTO, params?: RequestParams) =>
      this.request<ResultDTOOfPlexAccountDTO, ResultDTO>(`/api/PlexAccount/${id}`, "PUT", params, data),

    /**
     * @tags PlexAccount
     * @name PlexAccount_Delete
     * @request DELETE:/api/PlexAccount/{id}
     */
    plexAccountDelete: (id: number, params?: RequestParams) =>
      this.request<ResultDTOOfBoolean, ResultDTO>(`/api/PlexAccount/${id}`, "DELETE", params),

    /**
     * @tags PlexAccount
     * @name PlexAccount_Validate
     * @request POST:/api/PlexAccount/validate
     */
    plexAccountValidate: (data: CredentialsDTO, params?: RequestParams) =>
      this.request<ResultDTOOfBoolean, ResultDTO>(`/api/PlexAccount/validate`, "POST", params, data),

    /**
     * @tags PlexAccount
     * @name PlexAccount_CheckUsername
     * @request GET:/api/PlexAccount/check/{username}
     */
    plexAccountCheckUsername: (username: string | null, params?: RequestParams) =>
      this.request<ResultDTOOfBoolean, ResultDTO>(`/api/PlexAccount/check/${username}`, "GET", params),

    /**
     * @tags PlexLibrary
     * @name PlexLibrary_Get
     * @request GET:/api/PlexLibrary/{id}
     */
    plexLibraryGet: (id: number, query?: { plexAccountId?: number }, params?: RequestParams) =>
      this.request<ResultDTOOfPlexLibraryDTO, ResultDTO>(
        `/api/PlexLibrary/${id}${this.addQueryParams(query)}`,
        "GET",
        params,
      ),

    /**
     * @tags PlexLibrary
     * @name PlexLibrary_RefreshLibrary
     * @request POST:/api/PlexLibrary/refresh
     */
    plexLibraryRefreshLibrary: (data: RefreshPlexLibraryDTO, params?: RequestParams) =>
      this.request<ResultDTOOfPlexLibraryDTO, ResultDTO>(`/api/PlexLibrary/refresh`, "POST", params, data),

    /**
     * @tags PlexServer
     * @name PlexServer_Get
     * @request GET:/api/PlexServer/{id}
     */
    plexServerGet: (id: number, params?: RequestParams) =>
      this.request<ResultDTOOfPlexServerDTO, ResultDTO>(`/api/PlexServer/${id}`, "GET", params),

    /**
     * @tags Settings
     * @name Settings_Get
     * @request GET:/api/Settings/activeaccount
     */
    settingsGet: (params?: RequestParams) =>
      this.request<ResultDTOOfPlexAccountDTO, ResultDTO>(`/api/Settings/activeaccount`, "GET", params),

    /**
     * @tags Settings
     * @name Settings_Put
     * @request PUT:/api/Settings/activeaccount/{id}
     */
    settingsPut: (id: number, params?: RequestParams) =>
      this.request<ResultDTOOfPlexAccountDTO, ResultDTO>(`/api/Settings/activeaccount/${id}`, "PUT", params),
  };
}
