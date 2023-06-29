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
import IAppConfig from '@class/IAppConfig';

export default interface IStoreState {
	pageReady: boolean;
	config: IAppConfig;
	libraries: PlexLibraryDTO[];
	mediaUrls: IObjectUrl[];
	downloadTaskUpdateList: DownloadTaskDTO[];
	// Progress Service
	libraryProgress: LibraryProgress[];
	fileMergeProgressList: FileMergeProgress[];
	inspectServerProgress: InspectServerProgressDTO[];
	serverConnectionCheckStatusProgress: ServerConnectionCheckStatusProgressDTO[];
	syncServerProgress: SyncServerProgress[];
}
