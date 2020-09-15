import Log from 'consola';
import { LogLevel } from '@aspnet/signalr';
import { Observable, ReplaySubject, Subscription } from 'rxjs';
import { HubConnectionFactory, ConnectionOptions, ConnectionStatus, HubConnection } from '@ssv/signalr-client';
import { signalRProgressHubUrl } from '@api/baseApi';
import {
	DownloadProgress,
	LibraryProgress,
	DownloadTaskCreationProgress,
	DownloadStatusChanged,
	FileMergeProgress,
} from '@dto/mainApi';
import { takeWhile, finalize } from 'rxjs/operators';
import HealthService from '@service/healthService';
import globalService from '@service/globalService';

export class SignalrService {
	private _hubFactory: HubConnectionFactory = new HubConnectionFactory();

	private _progressHubConnectionState: ConnectionStatus = ConnectionStatus.disconnected;
	private _progressHubConnection: HubConnection<ProgressHub>;
	private _progressHubSubscription: Subscription | null = null;

	private _downloadStatusChangedSubject: ReplaySubject<DownloadStatusChanged> = new ReplaySubject<DownloadStatusChanged>();
	private _fileMergeProgressSubject: ReplaySubject<FileMergeProgress> = new ReplaySubject<FileMergeProgress>();
	private _libraryProgressSubject: ReplaySubject<LibraryProgress> = new ReplaySubject<LibraryProgress>();

	public constructor() {
		const options: ConnectionOptions = {
			logger: LogLevel.None,
			retry: {
				maximumAttempts: 0,
			},
		};
		this._hubFactory.create({
			key: 'ProgressHub',
			endpointUri: signalRProgressHubUrl,
			options,
		});

		this._progressHubConnection = this._hubFactory.get<ProgressHub>('ProgressHub');

		this.setupSubscriptions();
	}

	private setupSubscriptions(): void {
		this._progressHubConnection?.connectionState$.subscribe((connectionState) => {
			this._progressHubConnectionState = connectionState.status;
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

		globalService
			.getAxiosReady()
			.pipe(finalize(() => this.startProgressHubConnection()))
			.subscribe();

		// Disable connections when server is offline
		HealthService.getServerStatus().subscribe((status) => {
			if (status) {
				this.startProgressHubConnection();
			} else {
				this.stopProgressHubConnection();
			}
		});
	}

	public startProgressHubConnection(): void {
		if (this._progressHubConnectionState === ConnectionStatus.disconnected) {
			this._progressHubSubscription = this._progressHubConnection?.connect().subscribe(() => {
				Log.debug('ProgressHub is now connected!');
			});
		}
	}

	public stopProgressHubConnection(): void {
		if (this._progressHubConnectionState !== ConnectionStatus.disconnected) {
			this._progressHubSubscription = this._progressHubConnection?.disconnect().subscribe(() => {
				Log.debug('ProgressHub is now disconnected!');
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
