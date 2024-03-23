import { PlexMediaType } from '@dto/mainApi';

export default interface IDownloadPreview {
	id: number;
	title: string;
	type: PlexMediaType;
	size: number;
	children: IDownloadPreview[];
}
