import { DownloadTaskDTO, FileMergeProgress, PlexAccountDTO, PlexLibraryDTO, PlexServerDTO, SettingsModel } from '@dto/mainApi';
import IObjectUrl from '@interfaces/IObjectUrl';

export default interface IStoreState {
	accounts: PlexAccountDTO[];
	servers: PlexServerDTO[];
	downloads: DownloadTaskDTO[];
	libraries: PlexLibraryDTO[];
	mediaUrls: IObjectUrl[];
	settings: SettingsModel;
	fileMergeProgressList: FileMergeProgress[];
	downloadTaskUpdateList: DownloadTaskDTO[];
}
