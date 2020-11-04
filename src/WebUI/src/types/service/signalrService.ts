import Log from 'consola';
import { LogLevel } from '@aspnet/signalr';
import { Observable, ReplaySubject, Subscription } from 'rxjs';
import { HubConnectionFactory, ConnectionOptions, ConnectionStatus, HubConnection } from '@ssv/signalr-client';
import { signalRProgressHubUrl, signalRNotificationHubUrl } from '@api/baseApi';
import {
	DownloadProgress,
	LibraryProgress,
	DownloadTaskCreationProgress,
	DownloadStatusChanged,
	FileMergeProgress,
	NotificationUpdate,
} from '@dto/mainApi';
import { takeWhile } from 'rxjs/operators';
import HealthService from '@service/healthService';
import globalService from '@service/globalService';

export class SignalrService {
	private _hubFactory: HubConnectionFactory = new HubConnectionFactory();

	private _progressHubConnectionState: ConnectionStatus = ConnectionStatus.disconnected;
	private _progressHubConnection: HubConnection<ProgressHub>;
	private _progressHubSubscription: Subscription | null = null;

	private _notificationHubConnectionState: ConnectionStatus = ConnectionStatus.disconnected;
	private _notificationHubConnection: HubConnection<NotificationHub>;
	private _notificationHubSubscription: Subscription | null = null;

	private _downloadProgressSubject: ReplaySubject<DownloadProgress> = new ReplaySubject<DownloadProgress>();
	private _downloadStatusChangedSubject: ReplaySubject<DownloadStatusChanged> = new ReplaySubject<DownloadStatusChanged>();
	private _fileMergeProgressSubject: ReplaySubject<FileMergeProgress> = new ReplaySubject<FileMergeProgress>();
	private _libraryProgressSubject: ReplaySubject<LibraryProgress> = new ReplaySubject<LibraryProgress>();

	private _notificationUpdateSubject: ReplaySubject<NotificationUpdate> = new ReplaySubject<NotificationUpdate>();

	public constructor() {
		Log.info('Setting up SignalR Service');

		const options: ConnectionOptions = {
			logger: LogLevel.None,
			retry: {
				maximumAttempts: 0,
			},
		};
		this._hubFactory.create(
			{
				key: 'ProgressHub',
				endpointUri: signalRProgressHubUrl,
				options,
			},
			{
				key: 'NotificationHub',
				endpointUri: signalRNotificationHubUrl,
				options,
			},
		);

		this._progressHubConnection = this._hubFactory.get<ProgressHub>('ProgressHub');
		this._notificationHubConnection = this._hubFactory.get<NotificationHub>('NotificationHub');

		this.setupSubscriptions();
	}

	private setupSubscriptions(): void {
		this._progressHubConnection?.connectionState$.subscribe((connectionState) => {
			this._progressHubConnectionState = connectionState.status;
		});

		this._notificationHubConnection?.connectionState$.subscribe((connectionState) => {
			this._notificationHubConnectionState = connectionState.status;
		});

		this._progressHubConnection.on<DownloadProgress>('DownloadProgress').subscribe((data) => {
			this._downloadProgressSubject.next(data);
		});

		this._progressHubConnection.on<DownloadStatusChanged>('DownloadStatus').subscribe((data) => {
			this._downloadStatusChangedSubject.next(data);
		});

		this._progressHubConnection.on<FileMergeProgress>('FileMergeProgress').subscribe((data) => {
			this._fileMergeProgressSubject.next(data);
		});

		this._progressHubConnection.on<LibraryProgress>('LibraryProgress').subscribe((data) => {
			this._libraryProgressSubject.next(data);
		});

		this._notificationHubConnection.on<NotificationUpdate>('Notification').subscribe((data) => {
			this._notificationUpdateSubject.next(data);
		});

		globalService
			.getAxiosReady()
			// .pipe(finalize(() => this.startProgressHubConnection()))
			.subscribe(() => this.startProgressHubConnection());

		// Disable connections when server is offline
		// HealthService.getServerStatus().subscribe((status) => {
		// 	if (status) {
		// 		this.startProgressHubConnection();
		// 	} else {
		// 		this.stopProgressHubConnection();
		// 	}
		// });
	}

	public startProgressHubConnection(): void {
		if (this._progressHubConnectionState === ConnectionStatus.disconnected) {
			this._progressHubSubscription = this._progressHubConnection?.connect().subscribe(() => {
				Log.info('ProgressHub is now connected!');
			});
		}
	}

	public stopProgressHubConnection(): void {
		if (this._progressHubConnectionState !== ConnectionStatus.disconnected) {
			this._progressHubSubscription = this._progressHubConnection?.disconnect().subscribe(() => {
				Log.info('ProgressHub is now disconnected!');
			});
		}
	}

	public getDownloadTaskCreationProgress(): Observable<DownloadTaskCreationProgress> {
		return this._progressHubConnection
			.on<DownloadTaskCreationProgress>('DownloadTaskCreation')
			.pipe(takeWhile((data) => !data.isComplete));
	}

	public getDownloadProgress(): Observable<DownloadProgress> {
		return this._progressHubConnection.on<DownloadProgress>('DownloadProgress');
	}

	public getFileMergeProgress(): Observable<FileMergeProgress> {
		return this._fileMergeProgressSubject.asObservable();
	}

	public getDownloadStatus(): Observable<DownloadStatusChanged> {
		return this._downloadStatusChangedSubject.asObservable();
	}

	public getLibraryProgress(): Observable<LibraryProgress> {
		return this._libraryProgressSubject.asObservable();
	}

	public getNotificationUpdate(): Observable<NotificationUpdate> {
		return this._notificationUpdateSubject.asObservable();
	}
}

const signalrService = new SignalrService();
export default signalrService;

export interface ProgressHub {
	DownloadProgress: DownloadProgress;
	FileMergeProgress: FileMergeProgress;
	DownloadTaskCreation: DownloadTaskCreationProgress;
	DownloadStatus: DownloadStatusChanged;
	LibraryProgress: LibraryProgress;
}

export interface NotificationHub {
	Notification: NotificationUpdate;
}
