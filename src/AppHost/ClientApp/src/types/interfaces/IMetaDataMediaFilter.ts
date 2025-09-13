import type { VideoQuality } from '@dto';

export interface IMetaDataMediaFilter {
	countryId: number;
	roleId: number;
	genreId: number;
	quality: VideoQuality;
}
