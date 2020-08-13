import { DownloadTaskDTO, DownloadProgress } from '@dto/mainApi';

export default interface IDownloadRow extends DownloadTaskDTO, DownloadProgress {}
