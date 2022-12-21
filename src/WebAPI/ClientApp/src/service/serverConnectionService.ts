import { Context } from '@nuxt/types';
import { Observable, of } from 'rxjs';
import { map, switchMap, take, tap } from 'rxjs/operators';
import { PlexServerConnectionDTO, PlexServerStatusDTO } from '@dto/mainApi';
import IStoreState from '@interfaces/service/IStoreState';
import { BaseService } from '@service';
import ISetupResult from '@interfaces/service/ISetupResult';
import { checkPlexServerConnection, getPlexServerConnections } from '@api/plexServerConnectionApi';

export class ServerConnectionService extends BaseService {
	// region Constructor and Setup
	public constructor() {
		super('ServerConnectionService', {
			// Note: Each service file can only have "unique" state slices which are not also used in other service files
			stateSliceSelector: (state: IStoreState) => {
				return {
					serverConnections: state.serverConnections,
				};
			},
		});
	}

	public setup(nuxtContext: Context): Observable<ISetupResult> {
		super.setup(nuxtContext);
		return this.refreshPlexServerConnections().pipe(
			switchMap(() => of({ name: this._name, isSuccess: true })),
			take(1),
		);
	}

	// endregion

	// region Fetch

	public refreshPlexServerConnections(): Observable<PlexServerConnectionDTO[]> {
		return getPlexServerConnections().pipe(
			tap((plexServers) => {
				if (plexServers.isSuccess) {
					this.setStoreProperty('serverConnections', plexServers.value);
				}
			}),
			map(() => this.getStoreSlice<PlexServerConnectionDTO[]>('serverConnections')),
			take(1),
		);
	}

	// endregion

	public getServerConnectionsByServerId(plexServerId: number = 0): Observable<PlexServerConnectionDTO[]> {
		return this.stateChanged.pipe(
			map(() => this.getStoreSlice<PlexServerConnectionDTO[]>('serverConnections')),
			map((connections) =>
				connections.filter((connection) => (plexServerId > 0 ? connection.plexServerId === plexServerId : true)),
			),
		);
	}

	public getServerConnections(): Observable<PlexServerConnectionDTO[]> {
		return this.getStateChanged<PlexServerConnectionDTO[]>('serverConnections');
	}

	public getServerConnection(connectionId: number): Observable<PlexServerConnectionDTO | null> {
		return this.stateChanged.pipe(
			map(() => this.getStoreSlice<PlexServerConnectionDTO[]>('serverConnections')),
			map((connections) => connections.find((x) => x.id === connectionId) ?? null),
		);
	}

	public checkServerConnection(plexServerConnectionId: number): Observable<PlexServerStatusDTO | null> {
		return checkPlexServerConnection(plexServerConnectionId).pipe(
			map((serverStatus) => {
				if (serverStatus.isSuccess && serverStatus.value) {
					const serverConnections = this.getStoreSlice<PlexServerConnectionDTO[]>('serverConnections');
					const index = serverConnections.findIndex((x) => x.id === plexServerConnectionId);
					if (index === -1) {
						return serverStatus.value;
					}
					serverConnections[index].latestConnectionStatus = serverStatus.value;
					this.setState({ serverConnections }, 'Update server status for connection ' + plexServerConnectionId);
				}
				return serverStatus?.value ?? null;
			}),
		);
	}
}

const serverConnectionService = new ServerConnectionService();
export default serverConnectionService;
