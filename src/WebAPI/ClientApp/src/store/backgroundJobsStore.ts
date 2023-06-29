import { acceptHMRUpdate, defineStore } from 'pinia';
import { Observable, of, Subject } from 'rxjs';
import { map, take } from 'rxjs/operators';
import ISetupResult from '@interfaces/service/ISetupResult';
import { JobStatusUpdateDTO, JobTypes } from '@dto/mainApi';

export const useBackgroundJobsStore = defineStore('BackgroundJobsStore', () => {
	// State
	const state = reactive<{
		jobStatusObservable: Subject<JobStatusUpdateDTO>;
		jobStatusList: JobStatusUpdateDTO[];
	}>({
		jobStatusObservable: new Subject<JobStatusUpdateDTO>(),
		jobStatusList: [],
	});

	// Actions
	const actions = {
		setup(): Observable<ISetupResult> {
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
		getJobStatusUpdate: (jobType: JobTypes): Observable<JobStatusUpdateDTO> =>
			state.jobStatusObservable.pipe(
				map((): JobStatusUpdateDTO[] => state.jobStatusList.filter((jobStatus) => jobStatus.jobType === jobType)),
				map(
					(jobStatusList: JobStatusUpdateDTO[]) =>
						jobStatusList.sort(function compare(a, b) {
							const dateA = new Date(a.jobStartTime);
							const dateB = new Date(b.jobStartTime);
							// @ts-ignore
							return dateA - dateB;
						})[0],
				),
			),
	};

	return {
		...toRefs(readonly(state)),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useBackgroundJobsStore, import.meta.hot));
}
