import {
	DownloadTaskDTO,
	FileMergeProgress,
	InspectServerProgressDTO,
	LibraryProgress,
	PlexLibraryDTO,
	ServerConnectionCheckStatusProgressDTO,
	SyncServerProgress,
} from '@dto/mainApi';
import IObjectUrl from '@interfaces/IObjectUrl';
import IAlert from '@interfaces/IAlert';
import IAppConfig from '@class/IAppConfig';

export default interface IStoreState {
	pageReady: boolean;
	config: IAppConfig;
	libraries: PlexLibraryDTO[];
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
}
