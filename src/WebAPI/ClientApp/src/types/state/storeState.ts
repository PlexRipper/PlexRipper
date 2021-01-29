import { DownloadTaskContainerDTO, PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import IObjectUrl from '@interfaces/IObjectUrl';

export default interface StoreState {
	servers: PlexServerDTO[];
	downloads: DownloadTaskContainerDTO;
	libraries: PlexLibraryDTO[];
	mediaUrls: IObjectUrl[];
}
