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
import IAlert from '@interfaces/IAlert';

export default interface IStoreState {
	accounts: PlexAccountDTO[];
	servers: PlexServerDTO[];
	downloads: DownloadTaskDTO[];
	libraries: PlexLibraryDTO[];
	notifications: NotificationDTO[];
	alerts: IAlert[];
	mediaUrls: IObjectUrl[];
	settings: SettingsModel;
	helpIdDialog: string;
	fileMergeProgressList: FileMergeProgress[];
	downloadTaskUpdateList: DownloadTaskDTO[];
}
