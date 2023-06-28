import Log from 'consola';

import { HubConnection, HubConnectionBuilder, HubConnectionState, IHttpConnectionOptions, LogLevel } from '@microsoft/signalr';
import { distinctUntilChanged, filter, map, switchMap, take } from 'rxjs/operators';
import { useCypressSignalRMock } from 'cypress-signalr-mock';
import { Observable, of, Subject } from 'rxjs';
import { isEqual } from 'lodash-es';
import BackgroundJobsService from './backgroundJobsService';
import BaseService from './baseService';
import IStoreState from '@interfaces/service/IStoreState';

import {
	DownloadTaskDTO,
	FileMergeProgress,
	InspectServerProgressDTO,
	JobStatusUpdateDTO,
	LibraryProgress,
	MessageTypes,
	NotificationDTO,
	ServerConnectionCheckStatusProgressDTO,
	ServerDownloadProgressDTO,
	SyncServerProgress,
} from '@dto/mainApi';
import ISetupResult from '@interfaces/service/ISetupResult';
import IAppConfig from '@class/IAppConfig';
import { useNotificationsStore } from '~/store';

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
					serverConnectionCheckStatusProgress: state.serverConnectionCheckStatusProgress,
					syncServerProgress: state.syncServerProgress,
				};
			},
		});
	}

	public setup(appConfig: IAppConfig | null = null): Observable<ISetupResult> {
		super.setup(appConfig);
		return from(this.initializeHubs()).pipe(
			switchMap(() => of({ name: this._name, isSuccess: true })),
			take(1),
		);
	}

	private async initializeHubs(): Promise<void> {
		// Disable SignalR initialization in test mode
		if (this.isInTestMode()) {
			return Promise.resolve();
		}
		Log.debug('Setting up SignalR Service');
		const options: IHttpConnectionOptions = {
			logger: LogLevel.Information,
		};
		// Setup Connections
		const baseUrl = this._appConfig.baseUrl;
		this._progressHubConnection =
			useCypressSignalRMock('progress') ??
			new HubConnectionBuilder().withUrl(`${baseUrl}/progress`, options).withAutomaticReconnect().build();
		this._notificationHubConnection =
			useCypressSignalRMock('notifications') ??
			new HubConnectionBuilder().withUrl(`${baseUrl}/notifications`, options).withAutomaticReconnect().build();

		await this.setupSubscriptions();
		await this.startProgressHubConnection();
		await this.startNotificationHubConnection();
	}

	private setupSubscriptions(): void {
		this._progressHubConnection?.on(MessageTypes.DownloadTaskUpdate, (data: DownloadTaskDTO) => {
			this.updateStore('downloadTaskUpdateList', data);
		});

		this._progressHubConnection?.on(MessageTypes.ServerDownloadProgress, (data: ServerDownloadProgressDTO) => {
			// TODO Each subscription should work like this, every subscription here should pass its values to the designated services for that type
			this._serverDownloadProgress.next(data);
		});

		this._progressHubConnection?.on(MessageTypes.FileMergeProgress, (data: FileMergeProgress) => {
			this.updateStore('fileMergeProgressList', data);
		});

		this._progressHubConnection?.on(MessageTypes.LibraryProgress, (data: LibraryProgress) => {
			this.updateStore('libraryProgress', data);
		});

		this._progressHubConnection?.on(
			MessageTypes.ServerConnectionCheckStatusProgress,
			(data: ServerConnectionCheckStatusProgressDTO) => this.setServerConnectionCheckStatusProgress(data),
		);

		this._progressHubConnection?.on(MessageTypes.InspectServerProgress, (data: InspectServerProgressDTO) =>
			this.setInspectServerProgress(data),
		);

		this._progressHubConnection?.on(MessageTypes.SyncServerProgress, (data: SyncServerProgress) => {
			this.updateStore('syncServerProgress', data);
		});

		this._progressHubConnection?.on(MessageTypes.JobStatusUpdate, (data: JobStatusUpdateDTO) => {
			BackgroundJobsService.setStatusJobUpdate(data);
		});

		this._notificationHubConnection?.on(MessageTypes.Notification, (data: NotificationDTO) => {
			// Notification slice is only updated in the notificationService.ts, we send it there.
			useNotificationsStore().setNotification(data);
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

	public getAllFileMergeProgress(): Observable<FileMergeProgress[]> {
		return this.stateChanged.pipe(
			map((x) => x?.fileMergeProgressList ?? []),
			distinctUntilChanged(isEqual),
		);
	}

	public getAllInspectServerProgress(): Observable<InspectServerProgressDTO[]> {
		return this.stateChanged.pipe(
			map((x) => x?.inspectServerProgress ?? []),
			distinctUntilChanged(isEqual),
		);
	}

	public getAllServerConnectionProgress(): Observable<ServerConnectionCheckStatusProgressDTO[]> {
		return this.stateChanged.pipe(
			map((x) => x?.serverConnectionCheckStatusProgress ?? []),
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

	public getServerConnectionProgress(
		plexServerConnectionId: number,
	): Observable<ServerConnectionCheckStatusProgressDTO | null> {
		return this.getAllServerConnectionProgress().pipe(
			map((x) => x?.find((x) => x.plexServerConnectionId === plexServerConnectionId) ?? null),
			filter((progress) => !!progress),
			distinctUntilChanged(isEqual),
		);
	}

	public getServerConnectionProgressByPlexServerId(plexServerId: number): Observable<ServerConnectionCheckStatusProgressDTO[]> {
		return this.getAllServerConnectionProgress().pipe(
			map((x) => x?.filter((y) => y.plexServerId === plexServerId)),
			distinctUntilChanged(isEqual),
		);
	}

	public getInspectServerProgress(plexServerId: number): Observable<InspectServerProgressDTO | null> {
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

	// endregion
	// region SetUpdate

	public setInspectServerProgress(data: InspectServerProgressDTO): void {
		this.updateStore('inspectServerProgress', data, 'plexServerId');
	}

	public setServerConnectionCheckStatusProgress(data: ServerConnectionCheckStatusProgressDTO): void {
		this.updateStore('serverConnectionCheckStatusProgress', data, 'plexServerConnectionId');
	}

	// endregion
}

export default new SignalrService();
