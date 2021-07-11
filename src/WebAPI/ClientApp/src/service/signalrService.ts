import Log from 'consola';
import { BaseService, GlobalService } from '@service';
import { LogLevel } from '@aspnet/signalr';
import { Observable, ReplaySubject, Subscription } from 'rxjs';
import { HubConnectionFactory, ConnectionOptions, ConnectionStatus, HubConnection } from '@ssv/signalr-client';
import {
	LibraryProgress,
	DownloadTaskCreationProgress,
	FileMergeProgress,
	NotificationDTO,
	PlexAccountRefreshProgress,
	DownloadTaskDTO,
} from '@dto/mainApi';
import { takeWhile } from 'rxjs/operators';
import { Context } from '@nuxt/types';

export class SignalrService extends BaseService {
	private _hubFactory: HubConnectionFactory = new HubConnectionFactory();

	private _progressHubConnectionState: ConnectionStatus = ConnectionStatus.disconnected;
	private _progressHubConnection: HubConnection<ProgressHub> | null = null;
	private _progressHubSubscription: Subscription | null = null;

	private _notificationHubConnectionState: ConnectionStatus = ConnectionStatus.disconnected;
	private _notificationHubConnection: HubConnection<NotificationHub> | null = null;
	private _notificationHubSubscription: Subscription | null = null;

	private _downloadTaskUpdateSubject: ReplaySubject<DownloadTaskDTO> = new ReplaySubject<DownloadTaskDTO>();
	private _downloadTaskCreationProgressSubject: ReplaySubject<DownloadTaskCreationProgress> = new ReplaySubject<DownloadTaskCreationProgress>();

	private _fileMergeProgressSubject: ReplaySubject<FileMergeProgress> = new ReplaySubject<FileMergeProgress>();
	private _libraryProgressSubject: ReplaySubject<LibraryProgress> = new ReplaySubject<LibraryProgress>();
	private _plexAccountRefreshProgressSubject: ReplaySubject<PlexAccountRefreshProgress> = new ReplaySubject<PlexAccountRefreshProgress>();

	private _NotificationUpdateSubject: ReplaySubject<NotificationDTO> = new ReplaySubject<NotificationDTO>();

	public constructor() {
		super({});
	}

	public setup(nuxtContext: Context): void {
		super.setup(nuxtContext);

		GlobalService.getConfigReady().subscribe((config) => {
			Log.info('Setting up SignalR Service');
			const options: ConnectionOptions = {
				logger: LogLevel.None,
				retry: {
					maximumAttempts: 0,
				},
			};

			const baseUrl = config.baseURL;
			this._hubFactory.create(
				{
					key: 'ProgressHub',
					endpointUri: `${baseUrl}/progress`,
					options,
				},
				{
					key: 'NotificationHub',
					endpointUri: `${baseUrl}/notifications`,
					options,
				},
			);

			this._progressHubConnection = this._hubFactory.get<ProgressHub>('ProgressHub');
			this._notificationHubConnection = this._hubFactory.get<NotificationHub>('NotificationHub');

			this.setupSubscriptions();
		});
	}

	private setupSubscriptions(): void {
		this._progressHubConnection?.connectionState$.subscribe((connectionState) => {
			this._progressHubConnectionState = connectionState.status;
		});

		this._notificationHubConnection?.connectionState$.subscribe((connectionState) => {
			this._notificationHubConnectionState = connectionState.status;
		});

		this._progressHubConnection?.on<DownloadTaskDTO>('DownloadTaskUpdate').subscribe((data) => {
			this._downloadTaskUpdateSubject.next(data);
		});

		this._progressHubConnection?.on<DownloadTaskCreationProgress>('DownloadTaskCreationProgress').subscribe((data) => {
			this._downloadTaskCreationProgressSubject.next(data);
		});

		this._progressHubConnection?.on<FileMergeProgress>('FileMergeProgress').subscribe((data) => {
			this._fileMergeProgressSubject.next(data);
		});

		this._progressHubConnection?.on<LibraryProgress>('LibraryProgress').subscribe((data) => {
			this._libraryProgressSubject.next(data);
		});

		this._progressHubConnection?.on<PlexAccountRefreshProgress>('PlexAccountRefreshProgress').subscribe((data) => {
			this._plexAccountRefreshProgressSubject.next(data);
		});

		this._notificationHubConnection?.on<NotificationDTO>('Notification').subscribe((data) => {
			this._NotificationUpdateSubject.next(data);
		});

		GlobalService.getAxiosReady()
			// .pipe(finalize(() => this.startProgressHubConnection()))
			.subscribe(() => {
				this.startProgressHubConnection();
				this.startNotificationHubConnection();
			});

		// Disable connections when server is offline
		// HealthService.getServerStatus().subscribe((status) => {
		// 	if (status) {
		// 		this.startProgressHubConnection();
		// 	} else {
		// 		this.stopProgressHubConnection();
		// 		this.stopNotificationHubConnection();
		// 	}
		// });
	}

	public startProgressHubConnection(): void {
		if (this._progressHubConnection && this._progressHubConnectionState === ConnectionStatus.disconnected) {
			this._progressHubSubscription = this._progressHubConnection.connect().subscribe(() => {
				Log.info('ProgressHub is now connected!');
			});
		}
	}

	public stopProgressHubConnection(): void {
		if (this._progressHubConnection && this._progressHubConnectionState !== ConnectionStatus.disconnected) {
			this._progressHubSubscription = this._progressHubConnection.disconnect().subscribe(() => {
				Log.info('ProgressHub is now disconnected!');
			});
		}
	}

	public startNotificationHubConnection(): void {
		if (this._notificationHubConnection && this._notificationHubConnectionState === ConnectionStatus.disconnected) {
			this._notificationHubSubscription = this._notificationHubConnection.connect().subscribe(() => {
				Log.info('NotificationHub is now connected!');
			});
		}
	}

	public stopNotificationHubConnection(): void {
		if (this._notificationHubConnection && this._notificationHubConnectionState !== ConnectionStatus.disconnected) {
			this._notificationHubSubscription = this._notificationHubConnection.disconnect().subscribe(() => {
				Log.info('NotificationHub is now disconnected!');
			});
		}
	}

	public getDownloadTaskCreationProgress(): Observable<DownloadTaskCreationProgress> {
		return this._downloadTaskCreationProgressSubject.asObservable().pipe(takeWhile((data) => !data.isComplete));
	}

	public getFileMergeProgress(): Observable<FileMergeProgress> {
		return this._fileMergeProgressSubject.asObservable();
	}

	public getDownloadTaskUpdate(): Observable<DownloadTaskDTO> {
		return this._downloadTaskUpdateSubject.asObservable();
	}

	public getLibraryProgress(): Observable<LibraryProgress> {
		return this._libraryProgressSubject.asObservable();
	}

	public getPlexAccountRefreshProgress(): Observable<PlexAccountRefreshProgress> {
		return this._plexAccountRefreshProgressSubject.asObservable();
	}

	public getNotificationUpdates(): Observable<NotificationDTO> {
		return this._NotificationUpdateSubject.asObservable();
	}
}

const signalrService = new SignalrService();
export default signalrService;

export interface ProgressHub {
	FileMergeProgress: FileMergeProgress;
	DownloadTaskCreation: DownloadTaskCreationProgress;
	DownloadTaskUpdate: DownloadTaskDTO;
	DownloadTaskCreationProgress: DownloadTaskCreationProgress;
	LibraryProgress: LibraryProgress;
	PlexAccountRefreshProgress: PlexAccountRefreshProgress;
}

export interface NotificationHub {
	Notification: NotificationDTO;
}
