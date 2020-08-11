import { PlexMediaType } from '@dto/mainApi';

export default interface ITreeViewItem {
	id: number;
	name: string;
	children: ITreeViewItem[];
	year?: number;
	type: PlexMediaType;
}
