import Log from 'consola';
import { acceptHMRUpdate, defineStore } from 'pinia';
import { computed, reactive, toRefs } from 'vue';
import type { Observable } from 'rxjs';
import { catchError, map, of, tap } from 'rxjs';
import type { ISetupResult } from '@interfaces';
import { StoreNames } from '@interfaces';
import { cloneDeep } from 'lodash-es';
import { useSignalrStore } from '@store';
import type { LiveLogEventDTO } from '@dto';
import { debugApi } from '@api';
import { SortDirection } from '@enums';

interface ILogsStoreState {
	logs: LiveLogEventDTO[];
	sortDirection: SortDirection;
}

export const useLogsStore = defineStore(StoreNames.LogsStore, () => {
	const defaultState: ILogsStoreState = {
		logs: [],
		sortDirection: SortDirection.Asc,
	};

	const state = reactive<ILogsStoreState>(cloneDeep(defaultState));
	const signalrStore = useSignalrStore();

	const actions = {
		setup(): Observable<ISetupResult> {
			// Subscribe to incoming live logs
			signalrStore.getLogEvents().pipe(
				tap((entry) => state.logs.push(entry)),
			).subscribe();

			return actions.refreshLogs().pipe(
				map((result) => ({ name: StoreNames.LogsStore, isSuccess: result.length > 0 })),
				catchError((error) => {
					Log.error('Failed to load cached logs', error);
					return of({ name: StoreNames.LogsStore, isSuccess: false });
				}),
			);
		},
		refreshLogs(): Observable<LiveLogEventDTO[]> {
			return debugApi.getAllLogsEndpoint().pipe(
				tap((result) => {
					if (result.isSuccess) {
						mergeSnapshot(result.value ?? []);
					}
				}),
				map(() => [...state.logs]),
			);
		},
		clearLogs(): void {
			state.logs = [];
		},
		setSortDirection(sortDirection: SortDirection): void {
			state.sortDirection = sortDirection;
		},
		$reset(): void {
			Object.assign(state, cloneDeep(defaultState));
		},
	};

	function mergeSnapshot(entries: LiveLogEventDTO[]): void {
		if (state.logs.length === 0) {
			state.logs = entries;
			return;
		}

		const existingSequences = new Set(state.logs.map((entry) => entry.sequence));
		const mergedEntries = [...entries];

		for (const entry of state.logs) {
			if (!existingSequences.has(entry.sequence)) {
				mergedEntries.push(entry);
			}
		}

		state.logs = mergedEntries.sort((left, right) => left.sequence - right.sequence);
	}

	const getters = {
		getSortedLogs: computed((): LiveLogEventDTO[] => {
			if (state.sortDirection === SortDirection.Desc) {
				return [...state.logs].reverse();
			}

			return state.logs;
		}),
	};

	return { ...toRefs(state), ...actions, ...getters };
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useLogsStore, import.meta.hot));
}
