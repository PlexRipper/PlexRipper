import Log from 'consola';
import { acceptHMRUpdate, defineStore } from 'pinia';
import type { Observable } from 'rxjs';
import { ReplaySubject, forkJoin, of } from 'rxjs';
import { filter, take, switchMap, tap } from 'rxjs/operators';
import type { ISetupResult } from '@interfaces';
import type {	CheckAllConnectionStatusUpdateDTO, DownloadJobUpdateDTO,
	FileMergeJobUpdateDTO, InspectPlexServerJobUpdateDTO,
	SyncServerMediaJobUpdateDTO,
} from '@dto';
import {
	JobStatus,
	JobTypes,
} from '@dto';
import { backgroundJobsApi, type JobStatusUpdateDTO } from '@api';
import { cloneDeep } from 'lodash-es';

interface IBackgroundJobsStore {
	jobStatusObservable: ReplaySubject<JobStatusUpdateDTO>;
	jobStatusList: JobStatusUpdateDTO[];
}

export const useBackgroundJobsStore = defineStore('BackgroundJobsStore', () => {
	// State
	const defaultState: IBackgroundJobsStore = {
		jobStatusObservable: new ReplaySubject<JobStatusUpdateDTO>(),
		jobStatusList: [],
	};

	const state = reactive<IBackgroundJobsStore>(cloneDeep(defaultState));

	const accountStore = useAccountStore();
	const serverStore = useServerStore();
	const libraryStore = useLibraryStore();
	const settingsStore = useSettingsStore();
	const connectionStore = useServerConnectionStore();

	// Actions
	const actions = {
		setup(): Observable<ISetupResult> {
			// Refresh accounts, servers, and settings on completion of the RefreshPlexServersAccessJob
			getters
				.getInspectPlexServerJobUpdate(JobStatus.Completed)
				.pipe(
					switchMap(() =>
						forkJoin([
							accountStore.refreshAccounts(),
							serverStore.refreshPlexServers(),
							libraryStore.refreshLibraries(),
							settingsStore.refreshSettings(),
						]),
					),
				)
				.subscribe();

			// Refresh the server connections on completion of the CheckPlexServerConnectionsJob
			getters
				.getCheckPlexServerConnectionsJobUpdate(JobStatus.Completed)
				.pipe(switchMap(() => connectionStore.refreshPlexServerConnections()))
				.subscribe();

			return backgroundJobsApi.getAllBackgroundJobsEndpoint().pipe(
				tap((response) => {
					for (const update of response.value ?? []) {
						switch (update.jobType) {
							case JobTypes.SyncServerMediaJob:
								actions.setStatusJobUpdate<SyncServerMediaJobUpdateDTO>({
									...update,
									data: JSON.parse(update.jsonString),
								});
								break;
							case JobTypes.DownloadJob:
								// string = DownloadTaskId (GUID)
								actions.setStatusJobUpdate<DownloadJobUpdateDTO>({
									...update,
									data: JSON.parse(update.jsonString),
								});
								break;
							case JobTypes.FileMergeJob:
								actions.setStatusJobUpdate<FileMergeJobUpdateDTO>({
									...update,
									data: JSON.parse(update.jsonString),
								});
								break;
							case JobTypes.InspectPlexServerJob:
								actions.setStatusJobUpdate<InspectPlexServerJobUpdateDTO>({
									...update,
									data: JSON.parse(update.jsonString),
								});
								break;
							case JobTypes.CheckAllConnectionsStatusByPlexServerJob:
								actions.setStatusJobUpdate<CheckAllConnectionStatusUpdateDTO>({
									...update,
									data: JSON.parse(update.jsonString),
								});
								break;
							default:
								throw new Error(`Unknown job type ${update.jobType}`);
						}
					}
				}),
				switchMap(() => of({ name: 'useBackgroundJobsStore', isSuccess: true }),
				), take(1));
		},

		setStatusJobUpdate<T>(jobStatusUpdate: JobStatusUpdateDTO<T>) {
			Log.debug(jobStatusUpdate);
			const i = state.jobStatusList.findIndex((x) => x.id === jobStatusUpdate.id);
			if (i > -1) {
				state.jobStatusList.splice(i, 1, jobStatusUpdate);
			} else {
				state.jobStatusList.push(jobStatusUpdate);
			}
			state.jobStatusObservable.next(jobStatusUpdate);
		},

		$reset() {
			Object.assign(state, cloneDeep(defaultState));
		},
	};

	// Getters
	const getters = {
		getJobStatusUpdate: (jobType: JobTypes, status: JobStatus | null = null): Observable<JobStatusUpdateDTO> =>
			state.jobStatusObservable.pipe(filter((x) => x.jobType === jobType && (status ? x.status === status : true))),
		getCheckPlexServerConnectionsJobUpdate: (
			status: JobStatus | null = null,
		): Observable<JobStatusUpdateDTO<CheckAllConnectionStatusUpdateDTO>> =>
			getters.getJobStatusUpdate(JobTypes.CheckAllConnectionsStatusByPlexServerJob, status),
		getInspectPlexServerJobUpdate: (status: JobStatus | null = null): Observable<JobStatusUpdateDTO<InspectPlexServerJobUpdateDTO>> =>
			getters.getJobStatusUpdate(JobTypes.InspectPlexServerJob, status),
		getSyncServerMediaJobUpdate: (status: JobStatus | null = null): Observable<JobStatusUpdateDTO<SyncServerMediaJobUpdateDTO>> =>
			getters.getJobStatusUpdate(JobTypes.SyncServerMediaJob, status),
	};

	return {
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useBackgroundJobsStore, import.meta.hot));
}
