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

import type { RequestParams } from "./http-client";

import type { JobStatusUpdateDTO } from "./data-contracts";

import { apiCheckPipe } from "@api/base";
import Axios from "axios";
import queryString from "query-string";
import { from } from "rxjs";

export class BackgroundJobs {
  /**
   * No description
   * * @tags Backgroundjobs
   * @name GetAllBackgroundJobsEndpoint
   * @request GET:/api/BackgroundJobs
   * @secure
   */
  getAllBackgroundJobsEndpoint = (params: RequestParams = {}) =>
    from(
      Axios.request<JobStatusUpdateDTO[]>({
        url: `/api/BackgroundJobs`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
    ).pipe(apiCheckPipe<JobStatusUpdateDTO[]>);
}

export class BackgroundJobsPaths {
  static getAllBackgroundJobsEndpoint = () => queryString.stringifyUrl({ url: `/api/BackgroundJobs` });
}
