import { Observable } from 'rxjs';
import { HubConnectionFactory } from '@ssv/signalr-client';
import { signalRDownloadProgressUrl } from '@api/baseApi';
import IDownloadProgress from '@dto/IDownloadProgress';
import { LogLevel } from '@aspnet/signalr';
import Log from 'consola';

export class SignalrService {
	private _hubFactory: HubConnectionFactory = new HubConnectionFactory();

	public constructor() {
		this._hubFactory.create({
			key: 'DownloadProgress',
			endpointUri: signalRDownloadProgressUrl,
			options: {
				logger: LogLevel.Debug,
			},
		});
	}

	public getDownloadProgress(): Observable<IDownloadProgress> {
		const hubConnection = this._hubFactory.get<DownloadHub>('DownloadProgress');
		hubConnection.connect().subscribe(() => {
			Log.debug(`DownloadHub is connected!`);
		});
		return hubConnection.on<IDownloadProgress>('DownloadProgress');
	}
}

const signalrService = new SignalrService();
export default signalrService;

export interface DownloadHub {
	DownloadProgress: IDownloadProgress;
}
