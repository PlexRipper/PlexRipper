import { PlexMediaType } from '@dto/mainApi';

export default interface ITreeViewItem {
	id: number;
	key: string;
	name: string;
	children: ITreeViewItem[];
	year?: number;
	type: PlexMediaType;
	item: any;
}
