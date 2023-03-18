import { PlexMediaType } from '@dto/mainApi';
import IMediaData from '@class/IMediaData';
import ITreeViewTableRow from '@interfaces/components/VTreeViewTable/ITreeViewTableRow';

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
	mediaType: PlexMediaType;
	actions: string[];
	mediaData: IMediaData[];
}
