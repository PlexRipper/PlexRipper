import IDownloadTask from '@dto/IDownloadTask';
import IDownloadProgress from '@dto/IDownloadProgress';

export default interface IDownloadRow extends IDownloadTask, IDownloadProgress {}
