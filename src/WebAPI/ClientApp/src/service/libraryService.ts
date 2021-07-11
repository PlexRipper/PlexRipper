import { iif, Observable, of } from 'rxjs';
import { BaseService, ServerService } from '@service';
import IStoreState from '@interfaces/IStoreState';
import { map, switchMap, tap } from 'rxjs/operators';
import { PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import { getPlexLibrary, refreshPlexLibrary } from '@api/plexLibraryApi';

export class LibraryService extends BaseService {
	public constructor() {
		super({
			stateSliceSelector: (state: IStoreState) => {
				return {
					libraries: state.libraries,
				};
			},
		});
	}

	private updateLibraryInStore(library: PlexLibraryDTO | null) {
		if (!library) {
			return;
		}
		// library = Object.freeze(library);
		const libraries = this.getState().libraries;
		const libraryIndex = libraries.findIndex((x) => x.id === library.id);
		if (libraryIndex === -1) {
			libraries.push(library);
		} else {
			libraries.splice(libraryIndex, 1, library);
		}
		this.setState({ libraries }, 'plexLibrary with id: ' + library.id + ' updated');
	}

	public getLibraries(): Observable<PlexLibraryDTO[]> {
		return this.stateChanged.pipe(switchMap((state: IStoreState) => of(state?.libraries ?? [])));
	}

	public getLibrary(libraryId: number): Observable<PlexLibraryDTO | null> {
		return this.getLibraries().pipe(
			map((libraries): PlexLibraryDTO | null => libraries.find((y) => y.id === libraryId) ?? null),
			switchMap((library) => iif(() => library !== null, of(library), this.retrieveLibrary(libraryId))),
		);
	}

	public retrieveLibrary(libraryId: number): Observable<PlexLibraryDTO | null> {
		return getPlexLibrary(libraryId, 0).pipe(tap((library) => this.updateLibraryInStore(library)));
	}

	public refreshLibrary(libraryId: number): void {
		refreshPlexLibrary(libraryId)
			.pipe(switchMap(() => this.retrieveLibrary(libraryId)))
			.subscribe((library) => this.updateLibraryInStore(library));
	}

	public getServerByLibraryID(libraryId: number): Observable<PlexServerDTO | undefined> {
		return ServerService.getServers().pipe(switchMap((x) => of(x.find((y) => y.plexLibraries.find((z) => z.id === libraryId)))));
	}
}

const libraryService = new LibraryService();
export default libraryService;
