import Log from 'consola';
import { Observable, iif, of, BehaviorSubject, combineLatest } from 'rxjs';
import { switchMap, take } from 'rxjs/operators';
import { PlexServerDTO, PlexServerStatusDTO } from '@dto/mainApi';
import AccountService from '@service/accountService';
import { checkPlexServer, getPlexServer, getPlexServers } from '@api/plexServerApi';
export class ServerService {
	private _servers: BehaviorSubject<PlexServerDTO[]> = new BehaviorSubject<PlexServerDTO[]>([]);

	public constructor() {
		AccountService.getActiveAccount()
			.pipe(switchMap((account) => iif(() => account == null, getPlexServers(), of(account?.plexServers ?? []))))
			.subscribe((data: PlexServerDTO[]) => {
				if (data) {
					this._servers.next(data);
				}
			});
	}

	public getServers(): Observable<PlexServerDTO[]> {
		return this._servers.asObservable();
	}

	public checkServer(plexServerId: number): void {
		if (plexServerId > 0) {
			combineLatest([this.getServers().pipe(take(1)), checkPlexServer(plexServerId)]).subscribe(
				(data: [PlexServerDTO[], PlexServerStatusDTO | null]) => {
					const servers = data[0];
					const serverStatus = data[1];
					if (serverStatus) {
						const index = servers.findIndex((x) => x.id === serverStatus.plexServerId);
						const server = servers[index];
						server.status = serverStatus;
						servers.splice(index, 1, server);
						this._servers.next(servers);
					}
				},
			);
		}
	}

	public getServer(plexServerId: number): void {
		if (plexServerId > 0) {
			combineLatest([this.getServers().pipe(take(1)), getPlexServer(plexServerId)]).subscribe(
				(data: [PlexServerDTO[], PlexServerDTO | null]) => {
					const servers = data[0];
					const server = data[1];

					if (server) {
						const index = servers.findIndex((x) => x.id === plexServerId);
						servers.splice(index, 1, server);
						this._servers.next(servers);
					}
				},
			);
		}
	}
}

const serverService = new ServerService();
export default serverService;
