import Log from 'consola';
import { Context } from '@nuxt/types';
import { Observable, of } from 'rxjs';
import { finalize, map, switchMap, take } from 'rxjs/operators';
import { PlexServerDTO, PlexServerStatusDTO } from '@dto/mainApi';
import { checkPlexServer, getPlexServers } from '@api/plexServerApi';
import IStoreState from '@interfaces/service/IStoreState';
import { BaseService, GlobalService } from '@service';
import ISetup from '@interfaces/ISetup';

export class ServerService extends BaseService implements ISetup {
	// region Constructor and Setup
	public constructor() {
		super('ServerService', {
			// Note: Each service file can only have "unique" state slices which are not also used in other service files
			stateSliceSelector: (state: IStoreState) => {
				return {
					servers: state.servers,
				};
			},
		});
	}

	public setup(nuxtContext: Context, callBack: (name: string) => void): void {
		super.setNuxtContext(nuxtContext);
		GlobalService.getAxiosReady()
			.pipe(
				finalize(() => this.fetchServers()),
				take(1),
			)
			.subscribe(() => callBack(this._name));
	}

	// endregion

	// region Fetch

	public fetchServers(): void {
		getPlexServers().subscribe((servers) => {
			if (servers.isSuccess) {
				Log.debug(`ServerService => Fetch Servers`, servers.value);
				this.setState({ servers: servers.value ?? [] }, 'Fetch Server Data');
			}
		});
	}

	// endregion

	public get servers(): PlexServerDTO[] {
		return this.getState().servers;
	}

	public getServers(): Observable<PlexServerDTO[]> {
		return this.stateChanged.pipe(switchMap((state: IStoreState) => of(state?.servers ?? [])));
	}

	public getServer(serverId: number): Observable<PlexServerDTO | null> {
		return this.getServers().pipe(switchMap((servers: PlexServerDTO[]) => of(servers.find((x) => x.id === serverId) ?? null)));
	}

	public checkServer(plexServerId: number): Observable<PlexServerStatusDTO | null> {
		return checkPlexServer(plexServerId).pipe(
			map((serverStatus) => {
				if (serverStatus.isSuccess && serverStatus.value) {
					const servers = this.getState().servers;
					const index = servers.findIndex((x) => x.id === serverStatus.value?.plexServerId);
					if (index === -1) {
						return serverStatus?.value ?? null;
					}
					const server = servers[index];
					server.status = serverStatus.value;
					servers.splice(index, 1, server);
					this.setState({ servers }, 'Update server status for ' + plexServerId);
				}
				return serverStatus?.value ?? null;
			}),
		);
	}
}

const serverService = new ServerService();
export default serverService;
