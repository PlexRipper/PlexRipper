import Log from 'consola';
import { Context } from '@nuxt/types';
import { Observable, of } from 'rxjs';
import { BaseService, GlobalService, ServerService } from '@service';
import IStoreState from '@interfaces/service/IStoreState';
import { distinctUntilChanged, filter, finalize, map, switchMap, take } from 'rxjs/operators';
import { PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import { getAllPlexLibraries, getPlexLibrary, refreshPlexLibrary, updateDefaultDestination } from '@api/plexLibraryApi';

export class LibraryService extends BaseService {
	// region Constructor and Setup

	public constructor() {
		super({
			stateSliceSelector: (state: IStoreState) => {
				return {
					libraries: state.libraries,
				};
			},
		});
	}

	public setup(nuxtContext: Context): void {
		super.setup(nuxtContext);

		GlobalService.getAxiosReady()
			.pipe(finalize(() => this.fetchLibraries()))
			.subscribe();
	}

	// endregion

	// region Fetch

	public fetchLibraries(): void {
		getAllPlexLibraries().subscribe((libraries) => {
			if (libraries.isSuccess) {
				Log.debug(`LibraryService => Fetch Libraries`, libraries.value);
				this.setState({ libraries: libraries.value }, 'Fetch Library Data');
			}
		});
	}

	public fetchLibrary(libraryId: number): void {
		getPlexLibrary(libraryId, 0)
			.pipe(take(1))
			.subscribe((library) => {
				if (library.isSuccess && library.value) {
					// We freeze library here as it doesnt have to be Vue reactive.
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
		return this.getLibraries().pipe(map((libraries): PlexLibraryDTO | null => libraries.find((y) => y.id === libraryId) ?? null));
	}

	public getServerByLibraryID(libraryId: number): Observable<PlexServerDTO | null> {
		return ServerService.getServers().pipe(
			switchMap((x: PlexServerDTO[]) => of(x.find((y) => y.plexLibraries.find((z) => z.id === libraryId)) ?? null)),
			filter((server) => server !== null),
			distinctUntilChanged(),
		);
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
					this.setState({ libraries }, `Updated library default destination with libraryId: ${libraryId} to ${folderPathId}`);
				}
			}
		});
	}
}

const libraryService = new LibraryService();
export default libraryService;
