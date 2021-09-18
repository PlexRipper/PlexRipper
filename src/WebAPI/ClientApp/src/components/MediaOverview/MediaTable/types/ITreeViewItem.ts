import { PlexMediaType } from '@dto/mainApi';
import ITreeViewTableRow from '@components/General/VTreeViewTable/ITreeViewTableRow';
import IMediaData from '@mediaOverview/MediaTable/types/IMediaData';

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
