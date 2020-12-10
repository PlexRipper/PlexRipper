import { DownloadTaskDTO, DownloadProgress, DownloadStatusChanged } from '@dto/mainApi';

export default interface IDownloadRow extends DownloadTaskDTO, DownloadProgress, DownloadStatusChanged {}
