import { Context } from '@nuxt/types';
import { Observable, of } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { PlexServerDTO } from '@dto/mainApi';
import { checkPlexServer, getPlexServers } from '@api/plexServerApi';
import IStoreState from '@interfaces/IStoreState';
import { BaseService, AccountService } from '@service';

export class ServerService extends BaseService {
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

		AccountService.getActiveAccount()
			.pipe(switchMap(() => getPlexServers()))
			.subscribe((data) => {
				if (data.isSuccess) {
					this.setState({ servers: data.value ?? [] }, 'Initial Server Data');
				}
			});
	}

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
					this.logHistory();
				}
			});
		}
	}
}

const serverService = new ServerService();
export default serverService;
