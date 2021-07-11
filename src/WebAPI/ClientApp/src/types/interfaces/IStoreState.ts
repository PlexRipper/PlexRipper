import {
	DownloadTaskDTO,
	FileMergeProgress,
	NotificationDTO,
	PlexAccountDTO,
	PlexLibraryDTO,
	PlexServerDTO,
	SettingsModel,
} from '@dto/mainApi';
import IObjectUrl from '@interfaces/IObjectUrl';

export default interface IStoreState {
	accounts: PlexAccountDTO[];
	servers: PlexServerDTO[];
	downloads: DownloadTaskDTO[];
	libraries: PlexLibraryDTO[];
	notifications: NotificationDTO[];
	alerts: any[];
	mediaUrls: IObjectUrl[];
	settings: SettingsModel;
	fileMergeProgressList: FileMergeProgress[];
	downloadTaskUpdateList: DownloadTaskDTO[];
}
