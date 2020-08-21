import { PlexMediaType } from '@dto/mainApi';

export default interface ITreeViewItem {
	id: number;
	key: string;
	title: string;
	year?: number;
	addedAt: string;
	updatedAt: string;
	type: PlexMediaType;
	children: ITreeViewItem[];
	item: any;
}
