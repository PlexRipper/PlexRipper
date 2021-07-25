import { iif, Observable, of } from 'rxjs';
import { BaseService, GlobalService, ServerService } from '@service';
import IStoreState from '@interfaces/IStoreState';
import { finalize, map, switchMap } from 'rxjs/operators';
import { PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import { getAllPlexLibraries, getPlexLibrary, refreshPlexLibrary, updateDefaultDestination } from '@api/plexLibraryApi';
import Log from 'consola';
import { Context } from '@nuxt/types';

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

	// endregion

	private updateLibraryInStore(library: PlexLibraryDTO | null) {
		if (!library) {
			return;
		}
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
		return getPlexLibrary(libraryId, 0).pipe(
			switchMap((library) => {
				if (library.isSuccess && library.value) {
					this.updateLibraryInStore(library.value);
				}
				return of(library?.value ?? null);
			}),
		);
	}

	public refreshLibrary(libraryId: number): void {
		refreshPlexLibrary(libraryId)
			.pipe(switchMap(() => this.retrieveLibrary(libraryId)))
			.subscribe((library) => this.updateLibraryInStore(library));
	}

	public getServerByLibraryID(libraryId: number): Observable<PlexServerDTO | null> {
		return ServerService.getServers().pipe(
			switchMap((x: PlexServerDTO[]) =>
				of(x.find((y: PlexServerDTO) => y.plexLibraries.find((z: PlexLibraryDTO) => z.id === libraryId)) ?? null),
			),
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
