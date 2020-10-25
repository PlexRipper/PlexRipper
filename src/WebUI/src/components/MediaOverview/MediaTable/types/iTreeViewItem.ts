import { PlexMediaType } from '@dto/mainApi';
import IMediaData from '@mediaOverview/MediaTable/types/IMediaData';

export default interface ITreeViewItem {
	id: number;
	key: string;
	title: string;
	year?: number;
	addedAt: string;
	updatedAt: string;
	type: PlexMediaType;
	mediaData: IMediaData[];
	children: ITreeViewItem[];
	item: any;
}
