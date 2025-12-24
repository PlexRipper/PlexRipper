import { acceptHMRUpdate, defineStore } from 'pinia';
import { reactive, toRefs } from 'vue';
import { forkJoin, type Observable } from 'rxjs';
import { of } from 'rxjs';
import { map, switchMap, tap } from 'rxjs/operators';
import { get } from '@vueuse/core';
import {
	type LibraryProgress, type LibrarySyncJobQueueDTO, LibrarySyncJobStatus, type PlexLibraryDTO, type PlexServerDTO,
} from '@dto';
import type { ISetupResult } from '@interfaces';
import { plexLibraryApi } from '@api';
import { RefreshDataType } from '@dto';
import { useBackgroundJobsStore, useServerStore, useSettingsStore, useSignalrStore } from '@store';
import { cloneDeep } from 'lodash-es';
import Log from 'consola';

interface ILibraryStoreState {
	libraries: PlexLibraryDTO[];
	syncQueues: LibrarySyncJobQueueDTO[];
	progress: LibraryProgress[];
}

export const useLibraryStore = defineStore('LibraryStore', () => {
	const defaultState: ILibraryStoreState = {
		libraries: [], syncQueues: [], progress: [],
	};

	const state = reactive<ILibraryStoreState>(cloneDeep(defaultState));

	const serverStore = useServerStore();
	const settingsStore = useSettingsStore();
	const signalRStore = useSignalrStore();
	const backgroundJobsStore = useBackgroundJobsStore();

	const actions = {
		setup(): Observable<ISetupResult> {
			// Listen for refresh notifications
			signalRStore.getRefreshNotification(RefreshDataType.PlexLibrary).pipe(switchMap(() => actions.refreshLibraries())).subscribe();
			signalRStore.getRefreshNotification(RefreshDataType.PlexLibrarySyncStatus).pipe(switchMap(() => actions.refreshLibrarySyncStatus())).subscribe();

			// Listen for library sync job status updates
			backgroundJobsStore.getLibrarySyncJobUpdate().subscribe((update) => {
				actions.updateSyncQueue(update.data);
			});

			return forkJoin([actions.refreshLibraries(), actions.refreshLibrarySyncStatus()]).pipe(switchMap(() => of({
				name: 'useLibraryStore', isSuccess: true,
			})));
		},
		updateSyncQueue(queue: LibrarySyncJobQueueDTO): void {
			const index = state.syncQueues.findIndex((x) => x.plexLibraryId === queue.plexLibraryId);
			if (index > -1) {
				state.syncQueues.splice(index, 1, queue);
			} else {
				state.syncQueues.push(queue);
			}
		},
		updateLibraryProgress(progress: LibraryProgress): void {
			const index = state.progress.findIndex((x) => x.id === progress.id);
			if (index > -1) {
				state.progress.splice(index, 1, progress);
			} else {
				state.progress.push(progress);
			}
		},
		refreshLibraries(): Observable<PlexLibraryDTO[]> {
			return plexLibraryApi.getAllPlexLibrariesEndpoint().pipe(tap(({ isSuccess, value }) => {
				if (isSuccess) {
					state.libraries = value ?? [];
				}
			}), map(() => get(getters.getLibraries())));
		},
		refreshLibrary(libraryId: number) {
			return plexLibraryApi.getPlexLibraryByIdEndpoint(libraryId).pipe(map(({ isSuccess, value }) => {
				if (isSuccess && value) {
					return value;
				}
				return null;
			}), tap((library) => actions.updateLibrary(library)));
		},
		refreshLibrarySyncStatus(): Observable<LibrarySyncJobQueueDTO[]> {
			return plexLibraryApi.getLibrarySyncStatusEndpoint().pipe(map(({ isSuccess, value }) => {
				if (isSuccess && value) {
					return value;
				}
				return [];
			}), tap((libraryQueues) => {
				state.syncQueues.splice(0, state.syncQueues.length, ...libraryQueues);
			}));
		},
		/**
     * Re-syncs a library by re-requesting all media from the Plex server.
     * @param libraryId
     */
		reSyncLibrary(libraryId: number): Observable<PlexLibraryDTO | null> {
			return plexLibraryApi.refreshLibraryMediaEndpoint(libraryId).pipe(tap((library) => actions.updateLibrary(library.value)), switchMap((library): Observable<PlexLibraryDTO | null> => of(getters.getLibrary(library.value?.id ?? 0))));
		},
		updateDefaultDestination(libraryId: number, folderPathId: number): void {
			plexLibraryApi.setPlexLibraryDefaultDestinationByIdEndpoint(libraryId, folderPathId).subscribe((result) => {
				if (result.isSuccess) {
					const index = state.libraries.findIndex((x) => x.id === libraryId);
					if (index > -1) {
						const updated = {
							...(state.libraries[index] as PlexLibraryDTO), defaultDestinationId: folderPathId,
						} as PlexLibraryDTO;
						state.libraries.splice(index, 1, updated);
					}
				}
			});
		}, updateLibrary(library?: PlexLibraryDTO | null): void {
			if (!library) {
				Log.error('Library was invalid, cannot update store.');
				return;
			}
			const i = state.libraries.findIndex((x) => x.id === library.id);
			if (i > -1) {
				// We freeze library here as it doesn't have to be Vue reactive.
				state.libraries.splice(i, 1, Object.freeze(library));
				return;
			}
			state.libraries.push(Object.freeze(library));
		}, $reset() {
			Object.assign(state, cloneDeep(defaultState));
		},
		/**
     * Clears completed sync queue items and their associated progress for all servers
     * where all items for that server are completed.
     */
		clearCompletedSyncQueues(): void {
			// Group queues by server
			const serverIds = [...new Set(state.syncQueues.map((x) => x.plexServerId))];
			const libraryIdsToClear: number[] = [];
			const serverIdsToClear: number[] = [];

			// Find servers where ALL items are completed
			for (const serverId of serverIds) {
				const serverQueues = state.syncQueues.filter((x) => x.plexServerId === serverId);
				if (serverQueues.length > 0 && serverQueues.every((x) => x.status === LibrarySyncJobStatus.Completed)) {
					serverIdsToClear.push(serverId);
					libraryIdsToClear.push(...serverQueues.map((x) => x.plexLibraryId));
				}
			}

			if (serverIdsToClear.length > 0) {
				// Remove completed items from the queue
				const remainingQueues = state.syncQueues.filter((x) => !serverIdsToClear.includes(x.plexServerId));
				state.syncQueues.splice(0, state.syncQueues.length, ...remainingQueues);

				// Remove progress items for cleared libraries
				const remainingProgress = state.progress.filter((x) => !libraryIdsToClear.includes(x.id));
				state.progress.splice(0, state.progress.length, ...remainingProgress);
			}
		},
	};
	const getters = {
		getLibrariesByServerId: (plexServerId: number) => state.libraries.filter((y) => y.plexServerId === plexServerId),
		getLibrary: (libraryId: number): PlexLibraryDTO | null => state.libraries.find((x) => x.id === libraryId) ?? null,
		getLibraries: (libraryIds: number[] = []): PlexLibraryDTO[] => {
			if (libraryIds.length === 0) {
				return state.libraries.map((x) => getters.getLibrary(x.id)).filter((x) => !!x);
			}
			return libraryIds.map((x) => getters.getLibrary(x)).filter((x) => !!x);
		},
		getServerByLibraryId: (libraryId: number): PlexServerDTO | null => {
			const library = state.libraries.find((x) => x.id === libraryId) ?? null;
			if (library) {
				return serverStore.getServer(library.plexServerId);
			}
			return null;
		},
		getLibraryName: (libraryId: number): string => {
			if (settingsStore.shouldMaskServerNames) {
				return '**MASKED**';
			}
			return getters.getLibrary(libraryId)?.title ?? '';
		},
		getLibraryProgress: (libraryId: number): LibraryProgress | null => {
			return state.progress.find((x) => x.id === libraryId) ?? null;
		},
		getIsLibrarySyncing: (libraryId: number): boolean => {
			return state.syncQueues.some((x) => x.plexLibraryId === libraryId && x.status == LibrarySyncJobStatus.Processing);
		},
		getLibrarySyncQueueGrouped: (): ILibrarySyncProgress[] => {
			return state.syncQueues.reduce<ILibrarySyncProgress[]>((acc, queue) => {
				const progress = state.progress.find((p) => p.id === queue.plexLibraryId);
				const queueWithProgress = { ...queue, ...progress } as LibrarySyncJobQueueDTO & LibraryProgress;
				const existing = acc.find((x) => x.serverId === queue.plexServerId);
				if (existing) {
					existing.progress.push(queueWithProgress);
				} else {
					acc.push({
						serverId: queue.plexServerId, progress: [queueWithProgress],
					});
				}
				return acc;
			}, []);
		},
	};
	return {
		...toRefs(state), ...actions, ...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useLibraryStore, import.meta.hot));
}

interface ILibrarySyncProgress {
	serverId: number;
	progress: (LibrarySyncJobQueueDTO & LibraryProgress)[];
}
