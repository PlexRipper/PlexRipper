import type { PlexMediaComparisonState } from '@dto';

export interface IMetaDataMediaFilter {
	countryId: number;
	roleId: number;
	genreId: number;
	qualityId: number;
	comparisonState: PlexMediaComparisonState | null;
}
