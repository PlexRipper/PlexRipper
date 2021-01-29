import { PlexMediaType } from '@dto/mainApi';
import ITreeViewTableRow from '@components/General/VTreeViewTable/ITreeViewTableRow';

export default interface ITreeViewItem extends ITreeViewTableRow {
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
}
