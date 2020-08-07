import { DownloadTaskDTO } from '@dto/mainApi';
import IDownloadProgress from '@dto/IDownloadProgress';

export default interface IDownloadRow extends DownloadTaskDTO, IDownloadProgress {}
