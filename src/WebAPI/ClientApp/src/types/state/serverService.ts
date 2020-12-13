import { Observable, of, iif } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { PlexLibraryDTO, PlexServerDTO, PlexServerStatusDTO } from '@dto/mainApi';
import { checkPlexServer, getPlexServers } from '@api/plexServerApi';
import StoreState from '@state/storeState';
import AccountService from '@service/accountService';
import { BaseService } from '@state/baseService';

export class ServerService extends BaseService {
	public constructor() {
		super((state: StoreState) => {
			return {
				servers: state.servers,
			};
		});

		AccountService.getActiveAccount()
			.pipe(switchMap((account) => iif(() => account == null, getPlexServers(), of(account?.plexServers ?? []))))
			.subscribe((data: PlexServerDTO[]) => {
				if (data) {
					this.setState({ servers: data }, 'Initial Server Data');
				}
			});
	}

	public get servers(): PlexServerDTO[] {
		return this.getState().servers;
	}

	public getServers(): Observable<PlexServerDTO[]> {
		return this.stateChanged.pipe(switchMap((state: StoreState) => of(state?.servers ?? [])));
	}

	public getServer(serverId: number): Observable<PlexServerDTO | null> {
		return this.getServers().pipe(switchMap((servers: PlexServerDTO[]) => of(servers.find((x) => x.id === serverId) ?? null)));
	}

	public checkServer(plexServerId: number): void {
		if (plexServerId > 0) {
			checkPlexServer(plexServerId).subscribe((serverStatus: PlexServerStatusDTO | null) => {
				if (serverStatus) {
					const servers = this.getState().servers;
					const index = servers.findIndex((x) => x.id === serverStatus.plexServerId);
					if (index === -1) {
						return;
					}
					const server = servers[index];
					server.status = serverStatus;
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
