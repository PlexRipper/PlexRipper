import { acceptHMRUpdate, defineStore } from 'pinia';
import { Observable, of, Subject } from 'rxjs';
import { filter, take, tap } from 'rxjs/operators';
import type { ISetupResult } from '@interfaces';
import { JobStatus, type JobStatusUpdateDTO, JobTypes } from '@dto';

export const useBackgroundJobsStore = defineStore('BackgroundJobsStore', () => {
	// State
	const state = reactive<{
		jobStatusObservable: Subject<JobStatusUpdateDTO>;
		jobStatusList: JobStatusUpdateDTO[];
	}>({
		jobStatusObservable: new Subject<JobStatusUpdateDTO>(),
		jobStatusList: [],
	});

	const accountStore = useAccountStore();
	const serverStore = useServerStore();
	const settingsStore = useSettingsStore();
	// Actions
	const actions = {
		setup(): Observable<ISetupResult> {
			// Refresh accounts, servers, and settings on completion of the job
			getters
				.getJobStatusUpdate(JobTypes.RefreshPlexServersAccessJob, JobStatus.Completed)
				.pipe(
					tap(() => accountStore.refreshAccounts()),
					tap(() => serverStore.refreshPlexServers()),
					tap(() => settingsStore.refreshSettings()),
				)
				.subscribe();

			return of({ name: useBackgroundJobsStore.name, isSuccess: true }).pipe(take(1));
		},

		setStatusJobUpdate(jobStatusUpdate: JobStatusUpdateDTO) {
			const i = state.jobStatusList.findIndex((x) => x.id === jobStatusUpdate.id);
			if (i > -1) {
				state.jobStatusList.splice(i, 1, jobStatusUpdate);
			} else {
				state.jobStatusList.push(jobStatusUpdate);
			}
			state.jobStatusObservable.next(jobStatusUpdate);
		},
	};

	// Getters
	const getters = {
		getJobStatusUpdate: (jobType: JobTypes, status: JobStatus | null = null): Observable<JobStatusUpdateDTO> =>
			state.jobStatusObservable.pipe(filter((x) => x.jobType === jobType && (status ? x.status === status : true))),
	};

	return {
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useBackgroundJobsStore, import.meta.hot));
}
