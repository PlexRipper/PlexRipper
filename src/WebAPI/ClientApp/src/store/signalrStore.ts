import { acceptHMRUpdate, defineStore } from 'pinia';
import type { Observable } from 'rxjs';
import { of, Subject, from } from 'rxjs';
import { distinctUntilChanged, filter, map, switchMap, take } from 'rxjs/operators';
import Log from 'consola';
import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import type { IHttpConnectionOptions, HubConnection } from '@microsoft/signalr';
import { useCypressSignalRMock } from 'cypress-signalr-mock';
import { isEqual } from 'lodash-es';
import type { ISetupResult } from '@interfaces';
import type {
	FileMergeProgress,
	InspectServerProgressDTO,
	LibraryProgress,
	NotificationDTO,
	ServerConnectionCheckStatusProgressDTO,
	ServerDownloadProgressDTO,
	SyncServerProgress,
	DataType } from '@dto';
import { MessageTypes } from '@dto';
import type IAppConfig from '@class/IAppConfig';
import type { JobStatusUpdateDTO } from '@api';
import { useDownloadStore } from '~/store/downloadStore';
import { useBackgroundJobsStore } from '~/store/backgroundJobsStore';
import { useNotificationsStore } from '~/store/notificationsStore';

export const useSignalrStore = defineStore('SignalrStore', () => {
	interface ISignalRStoreState {
		// Data
		libraryProgress: LibraryProgress[];
		syncServerProgress: SyncServerProgress[];
		fileMergeProgress: FileMergeProgress[];
		inspectServerProgress: InspectServerProgressDTO[];
		serverConnectionCheckStatusProgress: ServerConnectionCheckStatusProgressDTO[];
		// Subjects
		libraryProgressSubject: Subject<LibraryProgress[]>;
		syncServerProgressSubject: Subject<SyncServerProgress[]>;
		fileMergeProgressSubject: Subject<FileMergeProgress[]>;
		inspectServerProgressSubject: Subject<InspectServerProgressDTO[]>;
		serverConnectionCheckStatusProgressSubject: Subject<ServerConnectionCheckStatusProgressDTO[]>;
		refreshDataNotificationSubject: Subject<DataType>;
	}
	const state = reactive<ISignalRStoreState>({
		// Data
		libraryProgress: [],
		syncServerProgress: [],
		fileMergeProgress: [],
		inspectServerProgress: [],
		serverConnectionCheckStatusProgress: [],
		// Subjects
		libraryProgressSubject: new Subject<LibraryProgress[]>(),
		syncServerProgressSubject: new Subject<SyncServerProgress[]>(),
		fileMergeProgressSubject: new Subject<FileMergeProgress[]>(),
		inspectServerProgressSubject: new Subject<InspectServerProgressDTO[]>(),
		serverConnectionCheckStatusProgressSubject: new Subject<ServerConnectionCheckStatusProgressDTO[]>(),
		refreshDataNotificationSubject: new Subject<DataType>(),
	});

	// Connections
	let progressHubConnection: HubConnection | null;
	let notificationHubConnection: HubConnection | null;

	const actions = {
		setup(config: IAppConfig): Observable<ISetupResult> {
			return from(
				(async () => {
					Log.debug('Setting up SignalR Service');
					const options: IHttpConnectionOptions = {
						logger: LogLevel.Information,
					};
					// Setup Connections
					progressHubConnection
						= useCypressSignalRMock('progress', { enableForVitest: true })
						?? new HubConnectionBuilder()
							.withUrl(`${config.baseUrl}/progress`, options)
							.withAutomaticReconnect()
							.build();
					notificationHubConnection
						= useCypressSignalRMock('notifications', { enableForVitest: true })
						?? new HubConnectionBuilder()
							.withUrl(`${config.baseUrl}/notifications`, options)
							.withAutomaticReconnect()
							.build();

					setupSubscriptions();
					await startProgressHubConnection();
					await startNotificationHubConnection();
				})(),
			).pipe(
				switchMap(() => of({ name: useSignalrStore.name, isSuccess: true })),
				take(1),
			);
		},
	};

	function setupSubscriptions(): void {
		const downloadStore = useDownloadStore();
		const backgroundStore = useBackgroundJobsStore();
		const notificationsStore = useNotificationsStore();

		progressHubConnection?.on(MessageTypes.ServerDownloadProgress, (data: ServerDownloadProgressDTO) =>
			downloadStore.updateServerDownloadProgress(data),
		);

		progressHubConnection?.on(MessageTypes.FileMergeProgress, (data: FileMergeProgress) => {
			updateState<FileMergeProgress>('fileMergeProgress', data);
		});

		progressHubConnection?.on(MessageTypes.LibraryProgress, (data: LibraryProgress) => {
			updateState<LibraryProgress>('libraryProgress', data);
		});

		progressHubConnection?.on(
			MessageTypes.ServerConnectionCheckStatusProgress,
			(data: ServerConnectionCheckStatusProgressDTO) =>
				updateState<ServerConnectionCheckStatusProgressDTO>(
					'serverConnectionCheckStatusProgress',
					data,
					'plexServerConnectionId',
				),
		);

		progressHubConnection?.on(MessageTypes.InspectServerProgress, (data: InspectServerProgressDTO) =>
			updateState<InspectServerProgressDTO>('inspectServerProgress', data, 'plexServerId'),
		);

		progressHubConnection?.on(MessageTypes.SyncServerProgress, (data: SyncServerProgress) =>
			updateState<SyncServerProgress>('syncServerProgress', data),
		);

		progressHubConnection?.on(MessageTypes.JobStatusUpdate, (data: JobStatusUpdateDTO) =>
			backgroundStore.setStatusJobUpdate(data),
		);

		notificationHubConnection?.on(MessageTypes.Notification, (data: NotificationDTO) =>
			notificationsStore.setNotification(data),
		);

		notificationHubConnection?.on(MessageTypes.RefreshNotification, (data: DataType) => {
			state.refreshDataNotificationSubject.next(data);
		});
	}

	function updateState<T>(propertyName: keyof ISignalRStoreState, newObject: T, idName = 'id'): void {
		if (!state[propertyName]) {
			Log.error(`Failed to get ISignalRStoreState property name: ${propertyName}`);
			return;
		}

		if (isArray(newObject)) {
			for (const item of newObject) {
				update(item);
			}
		} else {
			update(newObject);
		}

		function update(item: T): void {
			if (!item[idName]) {
				Log.error(`Failed to find the correct id property in ${propertyName} with idName: ${idName}`, item);
				return;
			}

			const i = (state[propertyName] as Array<T>).findIndex((x) => x[idName] === item[idName]);
			if (i > -1) {
				(state[propertyName] as Array<T>).splice(i, 1, item);
			} else {
				(state[propertyName] as Array<T>).push(item);
			}
		}

		// Trigger Subject to send current state
		(state[propertyName + 'Subject'] as Subject<Array<T>>).next(state[propertyName] as Array<T>);
	}

	// region Start / Stop Hub Connections

	function startProgressHubConnection(): Promise<void> {
		if (progressHubConnection && progressHubConnection.state === HubConnectionState.Disconnected) {
			return progressHubConnection.start().then(() => {
				Log.info('ProgressHub is now connected!');
			});
		}
		return Promise.resolve();
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	function stopProgressHubConnection(): Promise<void> {
		if (progressHubConnection && progressHubConnection.state !== HubConnectionState.Disconnected) {
			return progressHubConnection.stop().then(() => {
				Log.info('ProgressHub is now disconnected!');
			});
		}
		return Promise.resolve();
	}

	function startNotificationHubConnection(): Promise<void> {
		if (notificationHubConnection && notificationHubConnection.state === HubConnectionState.Disconnected) {
			return notificationHubConnection.start().then(() => {
				Log.info('NotificationHub is now connected!');
			});
		}
		return Promise.resolve();
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	function stopNotificationHubConnection(): Promise<void> {
		if (notificationHubConnection && notificationHubConnection.state !== HubConnectionState.Disconnected) {
			return notificationHubConnection.stop().then(() => {
				Log.info('NotificationHub is now disconnected!');
			});
		}
		return Promise.resolve();
	}

	// endregion

	const getters = {
		// region Array Progress
		getAllLibraryProgress: (): Observable<LibraryProgress[]> => state.libraryProgressSubject.asObservable(),
		getAllSyncServerProgress: (): Observable<SyncServerProgress[]> => state.syncServerProgressSubject.asObservable(),
		getAllFileMergeProgress: (): Observable<FileMergeProgress[]> => state.fileMergeProgressSubject.asObservable(),
		getAllInspectServerProgress: (): Observable<InspectServerProgressDTO[]> =>
			state.inspectServerProgressSubject.asObservable(),
		getAllServerConnectionProgress: (): Observable<ServerConnectionCheckStatusProgressDTO[]> =>
			state.serverConnectionCheckStatusProgressSubject.asObservable(),
		// endregion

		// region Single Progress

		getLibraryProgress(libraryId: number): Observable<LibraryProgress> {
			return getters.getAllLibraryProgress().pipe(
				map((x) => x?.find((x) => x.id === libraryId) ?? null),
				filter((progress) => !!progress),
				distinctUntilChanged(isEqual),
			);
		},
		getSyncServerProgress(plexServerId: number): Observable<SyncServerProgress | null> {
			return getters.getAllSyncServerProgress().pipe(
				map((x) => x?.find((x) => x.id === plexServerId) ?? null),
				filter((progress) => !!progress),
				distinctUntilChanged(isEqual),
			);
		},
		getFileMergeProgress(id: number): Observable<FileMergeProgress | null> {
			return getters.getAllFileMergeProgress().pipe(
				map((x) => x?.find((x) => x.id === id) ?? null),
				filter((progress) => !!progress),
				distinctUntilChanged(isEqual),
			);
		},
		getInspectServerProgress(plexServerId: number): Observable<InspectServerProgressDTO | null> {
			return getters.getAllInspectServerProgress().pipe(
				map((x) => x?.find((x) => x.plexServerId === plexServerId) ?? null),
				filter((progress) => !!progress),
				distinctUntilChanged(isEqual),
			);
		},
		getServerConnectionProgressByPlexServerId(plexServerId: number): Observable<ServerConnectionCheckStatusProgressDTO[]> {
			return getters.getAllServerConnectionProgress().pipe(
				map((x) => x?.filter((y) => y.plexServerId === plexServerId)),
				distinctUntilChanged(isEqual),
			);
		},
		getRefreshNotification(filterOn: DataType): Observable<DataType> {
			return state.refreshDataNotificationSubject.asObservable().pipe(filter((x) => x === filterOn));
		},
		// endregion
	};
	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useSignalrStore, import.meta.hot));
}
