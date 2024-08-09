import { acceptHMRUpdate } from 'pinia';
import type { Observable } from 'rxjs';
import { of } from 'rxjs';
import { map, switchMap, tap } from 'rxjs/operators';
import { get } from '@vueuse/core';
import type { PlexLibraryDTO, PlexServerDTO } from '@dto';
import type { ISetupResult } from '@interfaces';
import { plexLibraryApi } from '@api';
import { DataType } from '@dto';
import { useServerStore, useSettingsStore, useSignalrStore } from '#build/imports';

export const useLibraryStore = defineStore('LibraryStore', () => {
	const state = reactive<{ libraries: PlexLibraryDTO[] }>({
		libraries: [],
	});

	const serverStore = useServerStore();
	const settingsStore = useSettingsStore();
	const signalRStore = useSignalrStore();

	const actions = {
		setup(): Observable<ISetupResult> {
			// Listen for refresh notifications
			signalRStore.getRefreshNotification(DataType.PlexLibrary).pipe(switchMap(() => actions.refreshLibraries())).subscribe();

			return actions.refreshLibraries().pipe(switchMap(() => of({ name: useLibraryStore.name, isSuccess: true })));
		},
		refreshLibraries(): Observable<PlexLibraryDTO[]> {
			return plexLibraryApi.getAllPlexLibrariesEndpoint().pipe(
				tap((plexLibraries) => {
					if (plexLibraries.value) {
						state.libraries = plexLibraries.value;
					}
				}),
				map(() => get(getters.getLibraries)),
			);
		},
		refreshLibrary(libraryId: number) {
			return plexLibraryApi.getPlexLibraryByIdEndpoint(libraryId).pipe(
				tap((library) => {
					if (library.isSuccess && library.value) {
						const i = state.libraries.findIndex((x) => x.id === libraryId);
						if (i > -1) {
							// We freeze library here as it doesn't have to be Vue reactive.
							state.libraries.splice(i, 1, Object.freeze(library.value));
						}
						state.libraries.push(Object.freeze(library.value));
					}
				}),
			);
		},
		/**
		 * Re-syncs a library by re-requesting all media from the Plex server.
		 * @param libraryId
		 */
		reSyncLibrary(libraryId: number): Observable<PlexLibraryDTO | null> {
			return plexLibraryApi.refreshLibraryMediaEndpoint(libraryId).pipe(
				tap((library) => {
					if (library.isSuccess && library.value) {
						const i = state.libraries.findIndex((x) => x.id === libraryId);
						if (i > -1) {
							state.libraries.splice(i, 1, library.value);
						}
						state.libraries.push(library.value);
					}
				}),
				switchMap((library): Observable<PlexLibraryDTO | null> => of(getters.getLibrary(library.value?.id ?? 0))),
			);
		},
		updateDefaultDestination(libraryId: number, folderPathId: number): void {
			plexLibraryApi.setPlexLibraryDefaultDestinationByIdEndpoint(libraryId, folderPathId).subscribe((result) => {
				if (result.isSuccess) {
					const index = state.libraries.findIndex((x) => x.id === libraryId);
					if (index > -1) {
						state.libraries.splice(index, 1, { ...state.libraries[index], defaultDestinationId: folderPathId });
					}
				}
			});
		},
	};
	const getters = {
		getLibrariesByServerId: (plexServerId: number) => state.libraries.filter((y) => y.plexServerId === plexServerId),
		getLibrary: (libraryId: number): PlexLibraryDTO | null => state.libraries.find((x) => x.id === libraryId) ?? null,
		getLibraries: computed(() => state.libraries),
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
	};
	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useLibraryStore, import.meta.hot));
}
