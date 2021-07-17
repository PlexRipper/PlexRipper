import {
	DownloadTaskDTO,
	FileMergeProgress, FolderPathDTO,
	NotificationDTO,
	PlexAccountDTO,
	PlexLibraryDTO,
	PlexServerDTO,
	SettingsModel,
} from '@dto/mainApi';
import IObjectUrl from '@interfaces/IObjectUrl';
import IAlert from '@interfaces/IAlert';
import { FolderPathService } from '~/service/folderPathService';

export default interface IStoreState {
	accounts: PlexAccountDTO[];
	servers: PlexServerDTO[];
	downloads: DownloadTaskDTO[];
	libraries: PlexLibraryDTO[];
	notifications: NotificationDTO[];
	folderPaths: FolderPathDTO[];
	alerts: IAlert[];
	mediaUrls: IObjectUrl[];
	settings: SettingsModel;
	helpIdDialog: string;
	fileMergeProgressList: FileMergeProgress[];
	downloadTaskUpdateList: DownloadTaskDTO[];
}
