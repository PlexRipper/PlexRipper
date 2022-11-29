import {
	DownloadTaskDTO,
	FileMergeProgress,
	FolderPathDTO,
	NotificationDTO,
	PlexAccountDTO,
	PlexLibraryDTO,
	PlexServerDTO,
	SettingsModelDTO,
	InspectServerProgress,
	SyncServerProgress,
	LibraryProgress,
	DownloadTaskCreationProgress,
	ServerDownloadProgressDTO,
} from '@dto/mainApi';
import IObjectUrl from '@interfaces/IObjectUrl';
import IAlert from '@interfaces/IAlert';
import IAppConfig from '@class/IAppConfig';

export default interface IStoreState extends SettingsModelDTO {
	pageReady: boolean;
	config: IAppConfig;
	accounts: PlexAccountDTO[];
	servers: PlexServerDTO[];
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
	inspectServerProgress: InspectServerProgress[];
	syncServerProgress: SyncServerProgress[];
	downloadTaskCreationProgress: DownloadTaskCreationProgress;
}
