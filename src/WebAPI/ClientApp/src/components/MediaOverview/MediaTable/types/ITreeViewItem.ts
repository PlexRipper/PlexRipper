import { PlexMediaType } from '@dto/mainApi';

export default interface ITreeViewItem {
	id: number;
	key: string;
	title: string;
	year: number;
	mediaSize: number;
	duration: number;
	hasThumb: boolean;
	hasArt: boolean;
	hasBanner: boolean;
	hasTheme: boolean;
	addedAt: string;
	updatedAt: string;
	plexLibraryId: number;
	plexServerId: number;
	type: PlexMediaType;
	children?: ITreeViewItem[];
}
