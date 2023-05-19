import { Context } from '@nuxt/types';
import { Observable, of } from 'rxjs';
import { map, switchMap, take, tap } from 'rxjs/operators';
import { BaseService, ServerService } from '@service';
import IStoreState from '@interfaces/service/IStoreState';
import { PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import { getAllPlexLibraries, getPlexLibrary, refreshPlexLibrary, updateDefaultDestination } from '@api/plexLibraryApi';
import ISetupResult from '@interfaces/service/ISetupResult';

export class LibraryService extends BaseService {
	// region Constructor and Setup

	public constructor() {
		super('LibraryService', {
			// Note: Each service file can only have "unique" state slices which are not also used in other service files
			stateSliceSelector: (state: IStoreState) => {
				return {
					libraries: state.libraries,
				};
			},
		});
	}

	public setup(nuxtContext: Context): Observable<ISetupResult> {
		super.setup(nuxtContext);
		return this.refreshLibraries().pipe(
			switchMap(() => of({ name: this._name, isSuccess: true })),
			take(1),
		);
	}

	// endregion

	// region Fetch

	public refreshLibraries(): Observable<PlexLibraryDTO[]> {
		return getAllPlexLibraries().pipe(
			tap((plexLibraries) => {
				if (plexLibraries.isSuccess) {
					this.setStoreProperty('libraries', plexLibraries.value);
				}
			}),
			switchMap(() => this.getLibraries()),
		);
	}

	public fetchLibrary(libraryId: number): void {
		getPlexLibrary(libraryId, 0)
			.pipe(take(1))
			.subscribe((library) => {
				if (library.isSuccess && library.value) {
					// We freeze library here as it doesn't have to be Vue reactive.
					this.updateStore('libraries', Object.freeze(library.value));
				}
			});
	}

	// endregion

	public getLibraries(): Observable<PlexLibraryDTO[]> {
		return this.stateChanged.pipe(switchMap((state: IStoreState) => of(state?.libraries ?? [])));
	}

	public getLibrary(libraryId: number): Observable<PlexLibraryDTO | null> {
		this.fetchLibrary(libraryId);
		return this.getLibraries().pipe(
			map((libraries): PlexLibraryDTO | null => libraries.find((y) => y.id === libraryId) ?? null),
		);
	}

	public getServerByLibraryId(libraryId: number): Observable<PlexServerDTO | null> {
		const libraries = this.getStoreSlice<PlexLibraryDTO[]>('libraries');
		if (libraries.length === 0) {
			return of(null);
		}
		const library = libraries.find((x) => x.id === libraryId);
		if (!library) {
			return of(null);
		}

		return ServerService.getServer(library.plexServerId);
	}

	public refreshLibrary(libraryId: number): Observable<PlexLibraryDTO | null> {
		return refreshPlexLibrary(libraryId).pipe(
			switchMap((library) => {
				if (library.isSuccess && library.value) {
					this.updateStore('libraries', library.value);
				}
				return this.getLibraries().pipe(map((libraries) => libraries.find((y) => y.id === libraryId) ?? null));
			}),
		);
	}

	public getLibrariesByServerId(plexServerId: number): Observable<PlexLibraryDTO[]> {
		return this.getLibraries().pipe(map((x) => x.filter((y) => y.plexServerId === plexServerId)));
	}

	public updateDefaultDestination(libraryId: number, folderPathId: number): void {
		updateDefaultDestination(libraryId, folderPathId).subscribe((result) => {
			if (result.isSuccess) {
				const libraries = this.getState().libraries;
				const libraryIndex = libraries.findIndex((x) => x.id === libraryId);
				if (libraryIndex > -1) {
					libraries[libraryIndex].defaultDestinationId = folderPathId;
					this.setState(
						{ libraries },
						`Updated library default destination with libraryId: ${libraryId} to ${folderPathId}`,
					);
				}
			}
		});
	}
}

const libraryService = new LibraryService();
export default libraryService;
