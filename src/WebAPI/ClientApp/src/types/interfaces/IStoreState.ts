import {
	DownloadTaskDTO,
	FileMergeProgress,
	FolderPathDTO,
	NotificationDTO,
	PlexAccountDTO,
	PlexAccountRefreshProgress,
	PlexLibraryDTO,
	PlexServerDTO,
	SettingsModel,
	InspectServerProgress,
} from '@dto/mainApi';
import IObjectUrl from '@interfaces/IObjectUrl';
import IAlert from '@interfaces/IAlert';

export default interface IStoreState {
	accounts: PlexAccountDTO[];
	servers: PlexServerDTO[];
	libraries: PlexLibraryDTO[];
	downloads: DownloadTaskDTO[];
	notifications: NotificationDTO[];
	folderPaths: FolderPathDTO[];
	alerts: IAlert[];
	mediaUrls: IObjectUrl[];
	settings: SettingsModel;
	helpIdDialog: string;
	downloadTaskUpdateList: DownloadTaskDTO[];
	// Progress Service
	fileMergeProgressList: FileMergeProgress[];
	accountRefreshProgress: PlexAccountRefreshProgress[];
	inspectServerProgress: InspectServerProgress[];
	plexAccountRefreshProgress: PlexAccountRefreshProgress[];
}
