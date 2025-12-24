import { acceptHMRUpdate, defineStore } from 'pinia';
import { reactive, toRefs } from 'vue';
import type { Observable } from 'rxjs';
import { from, of, Subject } from 'rxjs';
import { distinctUntilChanged, filter, map, switchMap, take } from 'rxjs/operators';
import Log from 'consola';
import type { HubConnection, IHttpConnectionOptions } from '@microsoft/signalr';
import { HttpTransportType, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { useCypressSignalRMock } from 'cypress-signalr-mock';
import { isEqual, cloneDeep, isArray } from 'lodash-es';
import type { ISetupResult } from '@interfaces';
import type {
	RefreshDataType,
	LibraryProgress,
	NotificationDTO,
	ServerConnectionCheckStatusProgressDTO,
	ServerDownloadProgressDTO,
	ServerDownloadProgressMessagePackDTO,
} from '@dto';
import { MessageTypes } from '@dto';
import type { IRetryPolicy } from '@microsoft/signalr/src/IRetryPolicy';
import { useDownloadStore, useBackgroundJobsStore, useNotificationsStore } from '@store';
import Axios from 'axios';
import { MessagePackHubProtocol } from '@microsoft/signalr-protocol-msgpack';

export const useSignalrStore = defineStore('SignalrStore', () => {
	interface ISignalRStoreState {
		// Data
		serverConnectionCheckStatusProgress: ServerConnectionCheckStatusProgressDTO[];
		// Subjects
		serverConnectionCheckStatusProgressSubject: Subject<ServerConnectionCheckStatusProgressDTO[]>;
		refreshDataNotificationSubject: Subject<RefreshDataType>;
	}

	const defaultState: ISignalRStoreState = {
		// Data
		serverConnectionCheckStatusProgress: [],

		// Subjects
		serverConnectionCheckStatusProgressSubject: new Subject<ServerConnectionCheckStatusProgressDTO[]>(),
		refreshDataNotificationSubject: new Subject<RefreshDataType>(),
	};

	const state = reactive<ISignalRStoreState>(cloneDeep(defaultState));
	const libraryStore = useLibraryStore();

	// Connections
	let progressHubConnection: HubConnection | null;
	let downloadHubConnection: HubConnection | null;
	let notificationHubConnection: HubConnection | null;

	const actions = {
		setup(): Observable<ISetupResult> {
			return from((async () => {
				Log.debug('Setting up SignalR Service');
				const options: IHttpConnectionOptions = {
					logMessageContent: false,
					skipNegotiation: true,
					logger: LogLevel.None,
					transport: HttpTransportType.WebSockets,
					withCredentials: true,
				};

				const retryPolicy: IRetryPolicy = {
					nextRetryDelayInMilliseconds: () => 2000,
				};

				const baseApiUrl = Axios.defaults.baseURL;
				// Setup Connections
				progressHubConnection = useCypressSignalRMock('progress', { enableForVitest: true }) ?? new HubConnectionBuilder()
					.configureLogging(LogLevel.None)
					.withUrl(`${baseApiUrl}/progress`, options)
					.withAutomaticReconnect(retryPolicy)
					.build();

				const mock = useCypressSignalRMock('download', { enableForVitest: true });
				if (mock) {
					downloadHubConnection = mock; // mock uses JSON internally
				} else {
					downloadHubConnection = new HubConnectionBuilder()
						.withHubProtocol(new MessagePackHubProtocol())
						.configureLogging(LogLevel.None)
						.withUrl(`${baseApiUrl}/download`, options)
						.withAutomaticReconnect(retryPolicy)
						.build();
				}

				notificationHubConnection = useCypressSignalRMock('notifications', { enableForVitest: true }) ?? new HubConnectionBuilder()
					.configureLogging(LogLevel.None)
					.withUrl(`${baseApiUrl}/notifications`, options)
					.withAutomaticReconnect(retryPolicy)
					.build();

				setupSubscriptions();

				await Promise.all([startDownloadHubConnection(), startProgressHubConnection(), startNotificationHubConnection()]);
			})()).pipe(switchMap(() => of({ name: 'useSignalrStore', isSuccess: true })), take(1));
		}, $reset() {
			Object.assign(state, cloneDeep(defaultState));
		},
	};

	function setupSubscriptions(): void {
		const downloadStore = useDownloadStore();
		const backgroundStore = useBackgroundJobsStore();
		const notificationsStore = useNotificationsStore();

		downloadHubConnection?.on(MessageTypes.ServerDownloadProgress, (rawData: ServerDownloadProgressMessagePackDTO) => {
			Log.debug(rawData);
			if (Array.isArray(rawData)) {
				downloadStore.updateServerDownloadProgress(toServerDownloadProgressDTO(rawData));
			} else {
				downloadStore.updateServerDownloadProgress(rawData);
			}
		});

		progressHubConnection?.on(MessageTypes.LibraryProgress, (data: LibraryProgress) => libraryStore.updateLibraryProgress(data));

		progressHubConnection?.on(MessageTypes.ServerConnectionCheckStatusProgress, (data: ServerConnectionCheckStatusProgressDTO) => updateState<ServerConnectionCheckStatusProgressDTO>('serverConnectionCheckStatusProgress', data, 'plexServerConnectionId'));

		progressHubConnection?.on(MessageTypes.JobStatusUpdate, (data) => backgroundStore.setStatusJobUpdate(data));

		notificationHubConnection?.on(MessageTypes.Notification, (data: NotificationDTO) => notificationsStore.setNotification(data));

		notificationHubConnection?.on(MessageTypes.RefreshNotification, (data: RefreshDataType) => {
			state.refreshDataNotificationSubject.next(data);
		});
	}

	function updateState<T>(propertyName: keyof ISignalRStoreState, newObject: T, idName: keyof T): void {
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
				Log.error(`Failed to find the correct id property in ${propertyName} with idName: ${String(idName)}`, item);
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

	async function startDownloadHubConnection(): Promise<void> {
		if (!downloadHubConnection || downloadHubConnection.state !== HubConnectionState.Disconnected) return;

		await downloadHubConnection.start();
		Log.info('DownloadHub connected');
	}

	async function startProgressHubConnection() {
		if (!progressHubConnection || progressHubConnection.state !== HubConnectionState.Disconnected) return;

		await progressHubConnection.start();
		Log.info('ProgressHub connected');
	}

	async function startNotificationHubConnection() {
		if (!notificationHubConnection || notificationHubConnection.state !== HubConnectionState.Disconnected) return;

		await notificationHubConnection.start();
		Log.info('NotificationHub connected');
	}

	// endregion

	const getters = {
		// region Array Progress
		getAllServerConnectionProgress: (): Observable<ServerConnectionCheckStatusProgressDTO[]> => state.serverConnectionCheckStatusProgressSubject.asObservable(), // endregion

		// region Single Progress

		getServerConnectionProgressByPlexServerId(plexServerId: number): Observable<ServerConnectionCheckStatusProgressDTO[]> {
			return getters.getAllServerConnectionProgress().pipe(map((x) => x?.filter((y) => y.plexServerId === plexServerId)), distinctUntilChanged(isEqual));
		},
		getRefreshNotification(filterOn: RefreshDataType): Observable<RefreshDataType> {
			return state.refreshDataNotificationSubject.asObservable().pipe(filter((x) => x === filterOn));
		}, // endregion
	};
	return {
		...toRefs(state), ...actions, ...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useSignalrStore, import.meta.hot));
}

function toServerDownloadProgressDTO(arr: ServerDownloadProgressMessagePackDTO): ServerDownloadProgressDTO | null {
	if (!Array.isArray(arr))
		return null;

	function mapDownload(item) {
		if (!Array.isArray(item))
			return null;

		return {
			id: item[0],
			title: item[1],
			mediaType: item[2],
			status: item[3],
			percentage: Number(item[4]),
			dataReceived: item[5],
			dataTotal: item[6],
			downloadSpeed: item[7],
			timeRemaining: item[8],
			children: Array.isArray(item[9]) ? item[9].map(mapDownload) : [],
		};
	}

	return {
		id: arr[0], downloadableTasksCount: arr[1], downloads: Array.isArray(arr[2]) ? arr[2].map(mapDownload) : [],
	};
}
