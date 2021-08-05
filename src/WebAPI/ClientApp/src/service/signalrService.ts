import Log from 'consola';
import { BaseService, GlobalService } from '@service';
import { LogLevel } from '@aspnet/signalr';
import { Observable, of, ReplaySubject, Subscription } from 'rxjs';
import { HubConnectionFactory, ConnectionOptions, ConnectionStatus, HubConnection } from '@ssv/signalr-client';
import {
	LibraryProgress,
	DownloadTaskCreationProgress,
	FileMergeProgress,
	NotificationDTO,
	DownloadTaskDTO,
	InspectServerProgress,
} from '@dto/mainApi';
import { map, switchMap, takeWhile } from 'rxjs/operators';
import { Context } from '@nuxt/types';
import IStoreState from '@interfaces/IStoreState';

export class SignalrService extends BaseService {
	private _hubFactory: HubConnectionFactory = new HubConnectionFactory();

	private _progressHubConnectionState: ConnectionStatus = ConnectionStatus.disconnected;
	private _progressHubConnection: HubConnection<ProgressHub> | null = null;
	private _progressHubSubscription: Subscription | null = null;

	private _notificationHubConnectionState: ConnectionStatus = ConnectionStatus.disconnected;
	private _notificationHubConnection: HubConnection<NotificationHub> | null = null;
	private _notificationHubSubscription: Subscription | null = null;

	private _downloadTaskUpdateSubject: ReplaySubject<DownloadTaskDTO> = new ReplaySubject<DownloadTaskDTO>();
	private _downloadTaskCreationProgressSubject: ReplaySubject<DownloadTaskCreationProgress> =
		new ReplaySubject<DownloadTaskCreationProgress>();

	private _libraryProgressSubject: ReplaySubject<LibraryProgress> = new ReplaySubject<LibraryProgress>();

	private _NotificationUpdateSubject: ReplaySubject<NotificationDTO> = new ReplaySubject<NotificationDTO>();

	public constructor() {
		super({
			stateSliceSelector: (state: IStoreState) => {
				return {
					fileMergeProgressList: state.fileMergeProgressList,
					inspectServerProgress: state.inspectServerProgress,
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
			this.updateStore('fileMergeProgressList', data);
		});

		this._progressHubConnection?.on<LibraryProgress>('LibraryProgress').subscribe((data) => {
			this._libraryProgressSubject.next(data);
		});

		this._progressHubConnection?.on<InspectServerProgress>('InspectServerProgress').subscribe((data) => {
			this.updateStore('inspectServerProgress', data, 'plexServerId');
		});

		this._notificationHubConnection?.on<NotificationDTO>('Notification').subscribe((data) => {
			this._NotificationUpdateSubject.next(data);
		});

		GlobalService.getAxiosReady().subscribe(() => {
			this.startProgressHubConnection();
			this.startNotificationHubConnection();
		});
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

	// region Array Progress

	public getAllFileMergeProgress(): Observable<FileMergeProgress[]> {
		return this.stateChanged.pipe(switchMap((x) => of(x?.fileMergeProgressList ?? [])));
	}

	public getAllInspectServerProgress(): Observable<InspectServerProgress[]> {
		return this.stateChanged.pipe(map((x) => x?.inspectServerProgress ?? []));
	}
	// endregion

	// region Single Progress

	public getFileMergeProgress(id: number): Observable<FileMergeProgress | null> {
		return this.getAllFileMergeProgress().pipe(map((x) => x?.find((x) => x.id === id) ?? null));
	}

	public getInspectServerProgress(plexServerId: number): Observable<InspectServerProgress | null> {
		return this.getAllInspectServerProgress().pipe(map((x) => x?.find((x) => x.plexServerId === plexServerId) ?? null));
	}

	// endregion

	public getDownloadTaskCreationProgress(): Observable<DownloadTaskCreationProgress> {
		return this._downloadTaskCreationProgressSubject.asObservable().pipe(takeWhile((data) => !data.isComplete));
	}

	public getDownloadTaskUpdate(): Observable<DownloadTaskDTO> {
		return this._downloadTaskUpdateSubject.asObservable();
	}

	public getLibraryProgress(): Observable<LibraryProgress> {
		return this._libraryProgressSubject.asObservable();
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
	InspectServerProgress: InspectServerProgress;
}

export interface NotificationHub {
	Notification: NotificationDTO;
}
