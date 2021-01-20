import { DownloadTaskDTO, PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import IObjectUrl from '@interfaces/IObjectUrl';

export default interface StoreState {
	servers: PlexServerDTO[];
	downloads: DownloadTaskDTO[];
	libraries: PlexLibraryDTO[];
	mediaUrls: IObjectUrl[];
}
