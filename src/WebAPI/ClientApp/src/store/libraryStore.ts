import { acceptHMRUpdate } from 'pinia';
import { Observable, of } from 'rxjs';
import { map, switchMap, take, tap } from 'rxjs/operators';
import { PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import { getAllPlexLibraries, getPlexLibrary, reSyncPlexLibrary, updateDefaultDestination } from '@api/plexLibraryApi';
import { useServerStore } from '#build/imports';
import ISetupResult from '@interfaces/service/ISetupResult';

export const useLibraryStore = defineStore('LibraryStore', {
	state: (): { libraries: PlexLibraryDTO[] } => ({
		libraries: [],
	}),
	actions: {
		setup(): Observable<ISetupResult> {
			return this.refreshLibraries().pipe(switchMap(() => of({ name: useLibraryStore.name, isSuccess: true })));
		},
		refreshLibraries(): Observable<PlexLibraryDTO[]> {
			return getAllPlexLibraries().pipe(
				tap((plexLibraries) => {
					if (plexLibraries.isSuccess) {
						this.libraries = plexLibraries.value ?? [];
					}
				}),
				map(() => this.getLibraries),
			);
		},
		refreshLibrary(libraryId: number) {
			return getPlexLibrary(libraryId).pipe(
				tap((library) => {
					if (library.isSuccess && library.value) {
						const i = this.libraries.findIndex((x) => x.id === libraryId);
						if (i > -1) {
							// We freeze library here as it doesn't have to be Vue reactive.
							this.libraries.splice(i, 1, Object.freeze(library.value));
						}
						this.libraries.push(Object.freeze(library.value));
					}
				}),
			);
		},
		/**
		 * Re-syncs a library by re-requesting all media from the Plex server.
		 * @param libraryId
		 */
		reSyncLibrary(libraryId: number): Observable<PlexLibraryDTO | null> {
			return reSyncPlexLibrary(libraryId).pipe(
				tap((library) => {
					if (library.isSuccess && library.value) {
						const i = this.libraries.findIndex((x) => x.id === libraryId);
						if (i > -1) {
							this.libraries.splice(i, 1, library.value);
						}
						this.libraries.push(library.value);
					}
				}),
				switchMap((library) => of(this.getLibrary(library.value?.id ?? 0))),
			);
		},
		updateDefaultDestination(libraryId: number, folderPathId: number): void {
			updateDefaultDestination(libraryId, folderPathId).subscribe((result) => {
				if (result.isSuccess) {
					const index = this.libraries.findIndex((x) => x.id === libraryId);
					if (index > -1) {
						this.libraries.splice(index, 1, { ...this.libraries[index], defaultDestinationId: folderPathId });
					}
				}
			});
		},
	},
	getters: {
		getLibrariesByServerId: (state) => (plexServerId: number) =>
			state.libraries.filter((y) => y.plexServerId === plexServerId),
		getLibrary:
			(state) =>
			(libraryId: number): PlexLibraryDTO | null =>
				state.libraries.find((x) => x.id === libraryId) ?? null,
		getLibraries: (state) => state.libraries,
		getServerByLibraryId:
			(state) =>
			(libraryId: number): PlexServerDTO | null => {
				const library = state.libraries.find((x) => x.id === libraryId) ?? null;
				if (library) {
					return useServerStore().getServer(library.plexServerId);
				}
				return null;
			},
	},
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useLibraryStore, import.meta.hot));
}
