import { DownloadTaskDTO, PlexServerDTO } from '@dto/mainApi';

export default interface StoreState {
	servers: PlexServerDTO[];
	downloads: DownloadTaskDTO[];
}
