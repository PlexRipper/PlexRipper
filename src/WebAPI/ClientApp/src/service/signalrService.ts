import Log from 'consola';
// eslint-disable-next-line import/named
import { HubConnection, HubConnectionBuilder, HubConnectionState, IHttpConnectionOptions, LogLevel } from '@microsoft/signalr';
import { from, Observable, of, Subject } from 'rxjs';
import { distinctUntilChanged, filter, map, switchMap, take } from 'rxjs/operators';
import { Context } from '@nuxt/types';
import { isEqual } from 'lodash-es';
import IStoreState from '@interfaces/service/IStoreState';
import { BaseService } from '@service';
import {
	DownloadTaskCreationProgress,
	DownloadTaskDTO,
	FileMergeProgress,
	InspectServerProgress,
	LibraryProgress,
	NotificationDTO,
	ServerDownloadProgressDTO,
	SyncServerProgress,
} from '@dto/mainApi';
import notificationService from '~/service/notificationService';
import ISetupResult from '@interfaces/service/ISetupResult';
import IAppConfig from '@class/IAppConfig';

export class SignalrService extends BaseService {
	private _progressHubConnection: HubConnection | null = null;
	private _notificationHubConnection: HubConnection | null = null;

	private _serverDownloadProgress = new Subject<ServerDownloadProgressDTO>();

	public constructor() {
		super('SignalrService', {
			// Note: Each service file can only have "unique" state slices which are not also used in other service files
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

	public setup(nuxtContext: Context, appConfig: IAppConfig | null = null): Observable<ISetupResult> {
		super.setup(nuxtContext, appConfig);
		return from(this.initializeHubs()).pipe(
			switchMap(() => of({ name: this._name, isSuccess: true })),
			take(1),
		);
	}

	private async initializeHubs(): Promise<void> {
		// Ensure we don't run any SignalR functionality due to it being tricky to setup. Might revisit later
		// TODO Re-enable when trying to test SignalR functionality
		// @ts-ignore
		if (window.jest || window.Cypress) {
			return Promise.resolve();
		}
		Log.debug('Setting up SignalR Service');
		const options: IHttpConnectionOptions = {
			logger: LogLevel.Information,
		};
		// Setup Connections
		const baseUrl = this._appConfig.baseURL;
		this._progressHubConnection = new HubConnectionBuilder()
			.withUrl(`${baseUrl}/progress`, options)
			.withAutomaticReconnect()
			.build();
		this._notificationHubConnection = new HubConnectionBuilder()
			.withUrl(`${baseUrl}/notifications`, options)
			.withAutomaticReconnect()
			.build();

		await this.setupSubscriptions();
		await this.startProgressHubConnection();
		await this.startNotificationHubConnection();
	}

	private setupSubscriptions(): void {
		this._progressHubConnection?.on('DownloadTaskUpdate', (data: DownloadTaskDTO) => {
			this.updateStore('downloadTaskUpdateList', data);
		});

		this._progressHubConnection?.on('ServerDownloadProgress', (data: ServerDownloadProgressDTO) => {
			// TODO Each subscription should work like this, every subscription here should pass its values to the designated services for that type
			this._serverDownloadProgress.next(data);
		});

		this._progressHubConnection?.on('DownloadTaskCreationProgress', (data: DownloadTaskCreationProgress) => {
			this.updateStore('downloadTaskCreationProgress', data);
		});

		this._progressHubConnection?.on('FileMergeProgress', (data: FileMergeProgress) => {
			this.updateStore('fileMergeProgressList', data);
		});

		this._progressHubConnection?.on('LibraryProgress', (data: LibraryProgress) => {
			this.updateStore('libraryProgress', data);
		});

		this._progressHubConnection?.on('InspectServerProgress', (data: InspectServerProgress) =>
			this.setInspectServerProgress(data),
		);

		this._progressHubConnection?.on('SyncServerProgress', (data: SyncServerProgress) => {
			this.updateStore('syncServerProgress', data);
		});

		this._notificationHubConnection?.on('Notification', (data: NotificationDTO) => {
			// Notification slice is only updated in the notificationService.ts, we send it there.
			notificationService.setNotification(data);
		});
	}

	// region Start / Stop Hub Connections

	public startProgressHubConnection(): Promise<void> {
		if (this._progressHubConnection && this._progressHubConnection.state === HubConnectionState.Disconnected) {
			return this._progressHubConnection.start().then(() => {
				Log.info('ProgressHub is now connected!');
			});
		}
		return Promise.resolve();
	}

	public stopProgressHubConnection(): Promise<void> {
		if (this._progressHubConnection && this._progressHubConnection.state !== HubConnectionState.Disconnected) {
			return this._progressHubConnection.stop().then(() => {
				Log.info('ProgressHub is now disconnected!');
			});
		}
		return Promise.resolve();
	}

	public startNotificationHubConnection(): Promise<void> {
		if (this._notificationHubConnection && this._notificationHubConnection.state === HubConnectionState.Disconnected) {
			return this._notificationHubConnection.start().then(() => {
				Log.info('NotificationHub is now connected!');
			});
		}
		return Promise.resolve();
	}

	public stopNotificationHubConnection(): Promise<void> {
		if (this._notificationHubConnection && this._notificationHubConnection.state !== HubConnectionState.Disconnected) {
			return this._notificationHubConnection.stop().then(() => {
				Log.info('NotificationHub is now disconnected!');
			});
		}
		return Promise.resolve();
	}

	// endregion

	// region Array Progress
	public getAllDownloadTaskUpdate(): Observable<DownloadTaskDTO[]> {
		return this.stateChanged.pipe(
			map((x) => x?.downloadTaskUpdateList ?? []),
			distinctUntilChanged(isEqual),
		);
	}

	public getAllServerDownloadProgress(): Observable<ServerDownloadProgressDTO[]> {
		return this.stateChanged.pipe(
			map((x) => x?.serverDownloads ?? []),
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

	public GetServerDownloadProgress() {
		return this._serverDownloadProgress.asObservable();
	}

	public getFileMergeProgress(id: number): Observable<FileMergeProgress | null> {
		return this.getAllFileMergeProgress().pipe(
			map((x) => x?.find((x) => x.id === id) ?? null),
			filter((progress) => !!progress),
			distinctUntilChanged(isEqual),
		);
	}

	public getServerDownloadProgress(id: number): Observable<ServerDownloadProgressDTO | null> {
		return this.getAllServerDownloadProgress().pipe(
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
			// @ts-ignore
			filter((progress) => !!progress),
			distinctUntilChanged(isEqual),
		);
	}

	public getDownloadTaskCreationProgress(): Observable<DownloadTaskCreationProgress> {
		return this.stateChanged.pipe(
			map((x) => x?.downloadTaskCreationProgress ?? null),
			filter((progress) => !!progress),
			distinctUntilChanged(isEqual),
		);
	}

	// endregion
	// region SetUpdate

	public setInspectServerProgress(data: InspectServerProgress): void {
		this.updateStore('inspectServerProgress', data, 'plexServerId');
	}

	// endregion
}

const signalrService = new SignalrService();
export default signalrService;
