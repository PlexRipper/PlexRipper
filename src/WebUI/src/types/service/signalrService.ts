import Log from 'consola';
import { LogLevel } from '@aspnet/signalr';
import { Observable, Subscription } from 'rxjs';
import { HubConnectionFactory, ConnectionOptions, ConnectionStatus, HubConnection } from '@ssv/signalr-client';
import { signalRDownloadProgressUrl, signalRPlexLibraryProgressUrl } from '@api/baseApi';
import { DownloadProgress, LibraryProgress, DownloadTaskCreationProgress, DownloadStatusChanged } from '@dto/mainApi';
import { takeWhile } from 'rxjs/operators';
import HealthService from '@service/healthService';
export class SignalrService {
	private _hubFactory: HubConnectionFactory = new HubConnectionFactory();

	private _libraryHubConnection: HubConnection<LibraryHub>;
	private _libraryHubConnectionstate: ConnectionStatus = ConnectionStatus.disconnected;
	private _libraryHubSubscription: Subscription | null = null;

	private _downloadHubConnectionstate: ConnectionStatus = ConnectionStatus.disconnected;
	private _downloadHubConnection: HubConnection<DownloadHub>;
	private _downloadHubSubscription: Subscription | null = null;

	public constructor() {
		const options: ConnectionOptions = {
			logger: LogLevel.Error,
			retry: {
				maximumAttempts: 5,
			},
		};
		this._hubFactory.create(
			{
				key: 'DownloadHub',
				endpointUri: signalRDownloadProgressUrl,
				options,
			},
			{
				key: 'LibraryHub',
				endpointUri: signalRPlexLibraryProgressUrl,
				options,
			},
		);
		// Disable connections when server is offline
		HealthService.getServerStatus().subscribe((status) => {
			if (status) {
				this.startDownloadHubConnection();
				this.startLibraryHubConnection();
			} else {
				this.stopDownloadHubConnection();
				this.stopLibraryHubConnection();
			}
		});

		this._downloadHubConnection = this._hubFactory.get<DownloadHub>('DownloadHub');
		this._libraryHubConnection = this._hubFactory.get<LibraryHub>('LibraryHub');

		this._downloadHubConnection?.connectionState$.subscribe((connectionState) => {
			this._downloadHubConnectionstate = connectionState.status;
		});

		this._libraryHubConnection?.connectionState$.subscribe((connectionState) => {
			this._libraryHubConnectionstate = connectionState.status;
		});
	}

	public startLibraryHubConnection(): void {
		if (this._libraryHubConnectionstate === ConnectionStatus.disconnected) {
			this._libraryHubSubscription = this._libraryHubConnection?.connect().subscribe(() => {
				Log.debug('LibraryHub is now connected!');
			});
		}
	}

	public stopLibraryHubConnection(): void {
		if (this._libraryHubConnectionstate !== ConnectionStatus.disconnected) {
			this._libraryHubSubscription = this._libraryHubConnection?.disconnect().subscribe(() => {
				Log.debug('LibraryHub is now disconnected!');
			});
		}
	}

	public startDownloadHubConnection(): void {
		if (this._downloadHubConnectionstate === ConnectionStatus.disconnected) {
			this._downloadHubSubscription = this._downloadHubConnection?.connect().subscribe(() => {
				Log.debug('DownloadHub is now connected!');
			});
		}
	}

	public stopDownloadHubConnection(): void {
		if (this._downloadHubConnectionstate !== ConnectionStatus.disconnected) {
			this._downloadHubSubscription = this._downloadHubConnection?.disconnect().subscribe(() => {
				Log.debug('DownloadHub is now disconnected!');
			});
		}
	}

	public getDownloadTaskCreationProgress(): Observable<DownloadTaskCreationProgress> {
		// this.startDownloadHubConnection();
		return this._downloadHubConnection
			.on<DownloadTaskCreationProgress>('DownloadTaskCreation')
			.pipe(takeWhile((data) => !data.isComplete));
	}

	public getDownloadProgress(): Observable<DownloadProgress> {
		// this.startDownloadHubConnection();
		return this._downloadHubConnection.on<DownloadProgress>('DownloadProgress');
	}

	public getDownloadStatus(): Observable<DownloadStatusChanged> {
		// this.startDownloadHubConnection();
		return this._downloadHubConnection.on<DownloadStatusChanged>('DownloadStatus');
	}

	public getLibraryProgress(): Observable<LibraryProgress> {
		// this.startLibraryHubConnection();
		return this._libraryHubConnection.on<LibraryProgress>('LibraryProgress');
	}
}

const signalrService = new SignalrService();
export default signalrService;

export interface DownloadHub {
	DownloadProgress: DownloadProgress;
	DownloadTaskCreation: DownloadTaskCreationProgress;
	DownloadStatus: DownloadStatusChanged;
}

export interface LibraryHub {
	LibraryProgress: LibraryProgress;
}
