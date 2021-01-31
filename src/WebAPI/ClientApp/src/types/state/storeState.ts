import { DownloadTaskDTO, PlexAccountDTO, PlexLibraryDTO, PlexServerDTO, SettingsModel } from '@dto/mainApi';
import IObjectUrl from '@interfaces/IObjectUrl';

export default interface StoreState {
	accounts: PlexAccountDTO[];
	servers: PlexServerDTO[];
	downloads: DownloadTaskDTO[];
	libraries: PlexLibraryDTO[];
	mediaUrls: IObjectUrl[];
	settings: SettingsModel;
}
