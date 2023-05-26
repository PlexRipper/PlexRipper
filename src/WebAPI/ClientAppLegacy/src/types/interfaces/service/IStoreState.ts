import {
	DownloadTaskDTO,
	FileMergeProgress,
	FolderPathDTO,
	InspectServerProgressDTO,
	JobStatusUpdateDTO,
	LibraryProgress,
	NotificationDTO,
	PlexAccountDTO,
	PlexLibraryDTO,
	PlexServerConnectionDTO,
	PlexServerDTO,
	ServerConnectionCheckStatusProgressDTO,
	ServerDownloadProgressDTO,
	SettingsModelDTO,
	SyncServerProgress,
} from '@dto/mainApi';
import IObjectUrl from '@interfaces/IObjectUrl';
import IAlert from '@interfaces/IAlert';
import IAppConfig from '@class/IAppConfig';

export default interface IStoreState extends SettingsModelDTO {
	pageReady: boolean;
	config: IAppConfig;
	accounts: PlexAccountDTO[];
	servers: PlexServerDTO[];
	serverConnections: PlexServerConnectionDTO[];
	libraries: PlexLibraryDTO[];
	serverDownloads: ServerDownloadProgressDTO[];
	notifications: NotificationDTO[];
	folderPaths: FolderPathDTO[];
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
