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
	DownloadTaskDTO,
	InspectServerProgress,
	SyncServerProgress,
} from '@dto/mainApi';
import { distinctUntilChanged, filter, map, takeWhile } from 'rxjs/operators';
import { Context } from '@nuxt/types';
import IStoreState from '@interfaces/IStoreState';
import { isEqual } from 'lodash';

export class SignalrService extends BaseService {
	private _hubFactory: HubConnectionFactory = new HubConnectionFactory();

	private _progressHubConnectionState: ConnectionStatus = ConnectionStatus.disconnected;
	private _progressHubConnection: HubConnection<ProgressHub> | null = null;
	private _progressHubSubscription: Subscription | null = null;

	private _notificationHubConnectionState: ConnectionStatus = ConnectionStatus.disconnected;
	private _notificationHubConnection: HubConnection<NotificationHub> | null = null;
	private _notificationHubSubscription: Subscription | null = null;

	private _downloadTaskCreationProgressSubject: ReplaySubject<DownloadTaskCreationProgress> =
		new ReplaySubject<DownloadTaskCreationProgress>();

	public constructor() {
		super({
			stateSliceSelector: (state: IStoreState) => {
				return {
					downloadTaskUpdateList: state.downloadTaskUpdateList,
					libraryProgress: state.libraryProgress,
					fileMergeProgressList: state.fileMergeProgressList,
					inspectServerProgress: state.inspectServerProgress,
					syncServerProgress: state.syncServerProgress,
					notifications: state.notifications,
				};
			},
		});
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
			// Connections
			this._progressHubConnection?.connectionState$.subscribe((connectionState) => {
				this._progressHubConnectionState = connectionState.status;
			});

			this._notificationHubConnection?.connectionState$.subscribe((connectionState) => {
				this._notificationHubConnectionState = connectionState.status;
			});

			this.setupSubscriptions();
		});
	}

	private setupSubscriptions(): void {
		this._progressHubConnection?.on<DownloadTaskDTO>('DownloadTaskUpdate').subscribe((data) => {
			this.updateStore('downloadTaskUpdateList', data);
		});

		this._progressHubConnection?.on<DownloadTaskCreationProgress>('DownloadTaskCreationProgress').subscribe((data) => {
			this._downloadTaskCreationProgressSubject.next(data);
		});

		this._progressHubConnection?.on<FileMergeProgress>('FileMergeProgress').subscribe((data) => {
			this.updateStore('fileMergeProgressList', data);
		});

		this._progressHubConnection?.on<LibraryProgress>('LibraryProgress').subscribe((data) => {
			this.updateStore('libraryProgress', data);
		});

		this._progressHubConnection?.on<InspectServerProgress>('InspectServerProgress').subscribe((data) => {
			this.updateStore('inspectServerProgress', data, 'plexServerId');
		});

		this._progressHubConnection?.on<SyncServerProgress>('SyncServerProgress').subscribe((data) => {
			this.updateStore('syncServerProgress', data);
		});

		this._progressHubConnection?.on<LibraryProgress>('LibraryProgress').subscribe((data) => {
			this.updateStore('libraryProgress', data);
		});

		this._notificationHubConnection?.on<NotificationDTO>('Notification').subscribe((data) => {
			this.updateStore('notifications', data);
		});

		GlobalService.getAxiosReady().subscribe(() => {
			this.startProgressHubConnection();
			this.startNotificationHubConnection();
		});
	}

	// region Start / Stop Hub Connections

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
	// endregion

	// region Array Progress
	public getAllDownloadTaskUpdate(): Observable<DownloadTaskDTO[]> {
		return this.stateChanged.pipe(
			map((x) => x?.downloadTaskUpdateList ?? []),
			distinctUntilChanged(isEqual),
		);
	}

	public getAllFileMergeProgress(): Observable<FileMergeProgress[]> {
		return this.stateChanged.pipe(
			map((x) => x?.fileMergeProgressList ?? []),
			distinctUntilChanged(isEqual),
		);
	}

	public getAllInspectServerProgress(): Observable<InspectServerProgress[]> {
		return this.stateChanged.pipe(
			map((x) => x?.inspectServerProgress ?? []),
			distinctUntilChanged(isEqual),
		);
	}

	public getAllSyncServerProgress(): Observable<SyncServerProgress[]> {
		return this.stateChanged.pipe(
			map((x) => x?.syncServerProgress ?? []),
			distinctUntilChanged(isEqual),
		);
	}

	public getAllLibraryProgress(): Observable<LibraryProgress[]> {
		return this.stateChanged.pipe(
			map((x) => x?.libraryProgress ?? []),
			distinctUntilChanged(isEqual),
		);
	}

	public getAllNotifications(): Observable<NotificationDTO[]> {
		return this.stateChanged.pipe(
			map((x) => x?.notifications ?? []),
			distinctUntilChanged(isEqual),
		);
	}

	// endregion

	// region Single Progress

	public getFileMergeProgress(id: number): Observable<FileMergeProgress | null> {
		return this.getAllFileMergeProgress().pipe(
			map((x) => x?.find((x) => x.id === id) ?? null),
			filter((progress) => !!progress),
			distinctUntilChanged(isEqual),
		);
	}

	public getInspectServerProgress(plexServerId: number): Observable<InspectServerProgress | null> {
		return this.getAllInspectServerProgress().pipe(
			map((x) => x?.find((x) => x.plexServerId === plexServerId) ?? null),
			filter((progress) => !!progress),
			distinctUntilChanged(isEqual),
		);
	}

	public getSyncServerProgress(plexServerId: number): Observable<SyncServerProgress | null> {
		return this.getAllSyncServerProgress().pipe(
			map((x) => x?.find((x) => x.id === plexServerId) ?? null),
			filter((progress) => !!progress),
			distinctUntilChanged(isEqual),
		);
	}

	public getLibraryProgress(libraryId: number): Observable<LibraryProgress> {
		return this.getAllLibraryProgress().pipe(
			map((x) => x?.find((x) => x.id === libraryId) ?? null),
			filter((progress) => !!progress),
			distinctUntilChanged(isEqual),
		);
	}

	public getDownloadTaskCreationProgress(): Observable<DownloadTaskCreationProgress> {
		return this._downloadTaskCreationProgressSubject.asObservable().pipe(takeWhile((data) => !data.isComplete));
	}

	// endregion
}

const signalrService = new SignalrService();
export default signalrService;

export interface ProgressHub {
	FileMergeProgress: FileMergeProgress;
	DownloadTaskCreation: DownloadTaskCreationProgress;
	DownloadTaskUpdate: DownloadTaskDTO;
	DownloadTaskCreationProgress: DownloadTaskCreationProgress;
	LibraryProgress: LibraryProgress;
	InspectServerProgress: InspectServerProgress;
	SyncServerProgress: SyncServerProgress;
}

export interface NotificationHub {
	Notification: NotificationDTO;
}
