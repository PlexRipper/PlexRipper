import { iif, Observable, of } from 'rxjs';
import { BaseService } from '@state/baseService';
import StoreState from '@state/storeState';
import { map, switchMap, tap } from 'rxjs/operators';
import { PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import { getPlexLibrary } from '@api/plexLibraryApi';
import serverService from '@state/serverService';

export class LibraryService extends BaseService {
	public constructor() {
		super((state: StoreState) => {
			return {
				libraries: state.libraries,
			};
		});

		// serverService
		// 	.getServers()
		// 	.pipe(switchMap((servers) => servers.flatMap((x) => x.plexLibraries)))
		// 	.subscribe((libraries) => {
		// 		Log.debug('LibraryService', libraries);
		// 	});
	}

	public getLibraries(): Observable<PlexLibraryDTO[]> {
		return this.stateChanged.pipe(switchMap((state: StoreState) => of(state?.libraries ?? [])));
	}

	public getLibrary(libraryId: number): Observable<PlexLibraryDTO | null> {
		return this.getLibraries().pipe(
			map((libraries): PlexLibraryDTO | null => libraries.find((y) => y.id === libraryId) ?? null),
			switchMap((library) => iif(() => library !== null, of(library), this.refreshLibrary(libraryId))),
		);
	}

	public refreshLibrary(libraryId: number): Observable<PlexLibraryDTO | null> {
		return getPlexLibrary(libraryId, 0).pipe(
			tap((library) => {
				if (!library) {
					return;
				}
				const libraries = this.getState().libraries;
				const libraryIndex = libraries.findIndex((x) => x.id === libraryId);
				if (libraryIndex === -1) {
					libraries.push(library);
				} else {
					libraries.splice(libraryIndex, 1, library);
				}
				this.setState({ libraries }, 'plexLibrary with id: ' + libraryId + ' updated');
				this.logHistory();
			}),
		);
	}

	public getServerByLibraryID(libraryId: number): Observable<PlexServerDTO | undefined> {
		return serverService.getServers().pipe(switchMap((x) => of(x.find((y) => y.plexLibraries.find((z) => z.id === libraryId)))));
	}
}

const libraryService = new LibraryService();
export default libraryService;
