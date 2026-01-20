import Log from 'consola';
import { acceptHMRUpdate, defineStore } from 'pinia';
import { reactive } from 'vue';
import type { Observable } from 'rxjs';
import { ReplaySubject, forkJoin, of } from 'rxjs';
import { filter, take, switchMap, tap } from 'rxjs/operators';
import { StoreNames, type ISetupResult } from '@interfaces';
import type {
	CheckAllConnectionStatusUpdateDTO, InspectPlexServerJobUpdateDTO,
	JobStatusUpdateDTO as ApiJobStatusUpdateDTO, LibrarySyncJobQueueDTO,
} from '@dto';
import {
	JobStatus,
	JobTypes,
} from '@dto';
import type { JobStatusUpdateDTO } from '@api';
import { backgroundJobsApi } from '@api';
import { cloneDeep } from 'lodash-es';

interface IBackgroundJobsStore {
	jobStatusObservable: ReplaySubject<JobStatusUpdateDTO>;
}

export const useBackgroundJobsStore = defineStore(StoreNames.BackgroundJobsStore, () => {
	// State
	const defaultState: IBackgroundJobsStore = {
		jobStatusObservable: new ReplaySubject<JobStatusUpdateDTO>(),
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
						actions.setStatusJobUpdate(update);
					}
				}),
				switchMap(() => of({ name: StoreNames.BackgroundJobsStore, isSuccess: true }),
				), take(1));
		},

		setStatusJobUpdate(update: ApiJobStatusUpdateDTO) {
			try {
				const updateWithData = {
					...update,
					data: JSON.parse(update.jsonString),
				};
				Log.debug(updateWithData);
				state.jobStatusObservable.next(updateWithData);
			} catch (e) {
				Log.error('setStatusJobUpdate => Failed to parse job update\'s jsonString', e);
			}
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
		getLibrarySyncJobUpdate: (status: JobStatus | null = null): Observable<JobStatusUpdateDTO<LibrarySyncJobQueueDTO>> =>
			getters.getJobStatusUpdate(JobTypes.LibrarySyncJob, status),
	};

	return {
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useBackgroundJobsStore, import.meta.hot));
}
