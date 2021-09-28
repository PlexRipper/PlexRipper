import Log from 'consola';
import { Context } from '@nuxt/types';
import { Observable, of } from 'rxjs';
import { finalize, switchMap } from 'rxjs/operators';
import { PlexServerDTO } from '@dto/mainApi';
import { checkPlexServer, getPlexServers } from '@api/plexServerApi';
import IStoreState from '@interfaces/service/IStoreState';
import { BaseService, GlobalService } from '@service';

export class ServerService extends BaseService {
	// region Constructor and Setup
	public constructor() {
		super({
			stateSliceSelector: (state: IStoreState) => {
				return {
					servers: state.servers,
				};
			},
		});
	}

	public setup(nuxtContext: Context): void {
		super.setup(nuxtContext);
		GlobalService.getAxiosReady()
			.pipe(finalize(() => this.fetchServers()))
			.subscribe();
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

	public checkServer(plexServerId: number): void {
		if (plexServerId > 0) {
			checkPlexServer(plexServerId).subscribe((serverStatus) => {
				if (serverStatus.isSuccess && serverStatus.value) {
					const servers = this.getState().servers;
					const index = servers.findIndex((x) => x.id === serverStatus.value?.plexServerId);
					if (index === -1) {
						return;
					}
					const server = servers[index];
					server.status = serverStatus.value;
					servers.splice(index, 1, server);
					this.setState({ servers }, 'Update server status for ' + plexServerId);
				}
			});
		}
	}
}

const serverService = new ServerService();
export default serverService;
