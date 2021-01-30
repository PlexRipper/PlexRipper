import { DownloadTaskDTO, DownloadProgress, DownloadStatusChanged, PlexMediaType } from '@dto/mainApi';

export default interface IDownloadRow extends DownloadTaskDTO, DownloadProgress, DownloadStatusChanged {
	type: PlexMediaType;
	children: IDownloadRow[];
	actions: string[];
}
