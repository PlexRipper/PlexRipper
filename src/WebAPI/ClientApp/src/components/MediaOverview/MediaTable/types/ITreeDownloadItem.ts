import { PlexMediaType } from '@dto/mainApi';

export default interface ITreeDownloadItem {
	id: number;
	children?: ITreeDownloadItem[];
}
