import {
	DownloadTaskDTO,
	FileMergeProgress,
	InspectServerProgressDTO,
	JobStatusUpdateDTO,
	LibraryProgress,
	PlexAccountDTO,
	PlexLibraryDTO,
	PlexServerConnectionDTO,
	PlexServerDTO,
	ServerConnectionCheckStatusProgressDTO,
	ServerDownloadProgressDTO,
	SyncServerProgress,
} from '@dto/mainApi';
import IObjectUrl from '@interfaces/IObjectUrl';
import IAlert from '@interfaces/IAlert';
import IAppConfig from '@class/IAppConfig';

export default interface IStoreState {
	pageReady: boolean;
	config: IAppConfig;
	accounts: PlexAccountDTO[];
	servers: PlexServerDTO[];
	serverConnections: PlexServerConnectionDTO[];
	libraries: PlexLibraryDTO[];
	serverDownloads: ServerDownloadProgressDTO[];
	alerts: IAlert[];
	mediaUrls: IObjectUrl[];
	helpIdDialog: string;
	downloadTaskUpdateList: DownloadTaskDTO[];
	// Progress Service
	libraryProgress: LibraryProgress[];
	fileMergeProgressList: FileMergeProgress[];
	inspectServerProgress: InspectServerProgressDTO[];
	serverConnectionCheckStatusProgress: ServerConnectionCheckStatusProgressDTO[];
	syncServerProgress: SyncServerProgress[];
	jobStatus: JobStatusUpdateDTO[];
}
