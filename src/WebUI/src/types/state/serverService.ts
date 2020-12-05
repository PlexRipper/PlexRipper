import { Observable, of, iif } from 'rxjs';
import { switchMap, take } from 'rxjs/operators';
import { PlexLibraryDTO, PlexServerDTO, PlexServerStatusDTO } from '@dto/mainApi';
import { checkPlexServer, getPlexServers } from '@api/plexServerApi';
import { ObservableStore } from '@codewithdan/observable-store';
import StoreState from '@state/storeState';
import AccountService from '@service/accountService';
import { getPlexLibrary } from '@api/plexLibraryApi';

export class ServerService extends ObservableStore<StoreState> {
	get servers(): PlexServerDTO[] {
		return this.getState().servers;
	}

	public constructor() {
		const initialState: StoreState = {
			servers: [],
		};
		ObservableStore.initializeState(initialState);

		super({
			trackStateHistory: true,
			stateSliceSelector: (state: StoreState) => {
				return {
					servers: state.servers,
				};
			},
		});

		AccountService.getActiveAccount()
			.pipe(switchMap((account) => iif(() => account == null, getPlexServers(), of(account?.plexServers ?? []))))
			.subscribe((data: PlexServerDTO[]) => {
				if (data) {
					this.setState({ servers: data });
				}
			});
	}

	public getServers(): Observable<PlexServerDTO[]> {
		return this.stateChanged.pipe(switchMap((state: StoreState) => of(state?.servers ?? [])));
	}

	public getServer(serverId: number): Observable<PlexServerDTO | null> {
		return this.getServers().pipe(switchMap((servers: PlexServerDTO[]) => of(servers.find((x) => x.id === serverId) ?? null)));
	}

	public getServerByLibraryID(libraryId: number): Observable<PlexServerDTO | undefined> {
		return this.getServers().pipe(switchMap((x) => of(x.find((y) => y.plexLibraries.find((z) => z.id === libraryId)))));
	}

	public getLibrary(libraryId: number): Observable<PlexLibraryDTO | null> {
		// Search for the library in the servers state
		const library: PlexLibraryDTO | null = this.findLibrary(this.servers, libraryId);

		// If the library has already been stored with media then return it.
		if (library && library.count > 0) {
			return of(library);
		}

		this.refreshLibrary(libraryId);
		return this.stateChanged.pipe(switchMap((x) => of(this.findLibrary(x?.servers ?? [], libraryId))));
	}

	public findLibrary(servers: PlexServerDTO[], libraryId: number): PlexLibraryDTO | null {
		if (!servers) {
			return null;
		}
		for (let i = 0; i < servers.length; i++) {
			if (servers[i] || servers[i].plexLibraries || servers[i].plexLibraries.length > 0) {
				for (let j = 0; i < servers[i].plexLibraries.length; j++) {
					if (servers[i].plexLibraries[j].id === libraryId) {
						return servers[i].plexLibraries[j];
					}
				}
			}
		}
		return null;
	}

	public refreshLibrary(libraryId: number): void {
		getPlexLibrary(libraryId ?? 0, 0)
			.pipe(take(1))
			.subscribe((libraryData) => {
				if (libraryData) {
					const servers = this.servers;
					const serverIndex = this.servers.findIndex((x) => x.id === libraryData.plexServerId);
					if (serverIndex === -1) {
						return;
					}
					const libraryIndex = servers[serverIndex].plexLibraries.findIndex((x) => x.id === libraryData.id);
					if (libraryIndex === -1) {
						return;
					}
					servers[serverIndex].plexLibraries.splice(libraryIndex, 1, libraryData);
					this.setState({ servers }, 'plexLibrary refresh');
				}
			});
	}

	public checkServer(plexServerId: number): void {
		if (plexServerId > 0) {
			checkPlexServer(plexServerId).subscribe((serverStatus: PlexServerStatusDTO | null) => {
				if (serverStatus) {
					const servers = this.servers;
					const index = servers.findIndex((x) => x.id === serverStatus.plexServerId);
					if (index === -1) {
						return;
					}
					const server = servers[index];
					server.status = serverStatus;
					servers.splice(index, 1, server);
					this.setState({ servers });
				}
			});
		}
	}
}

const serverService = new ServerService();
export default serverService;
