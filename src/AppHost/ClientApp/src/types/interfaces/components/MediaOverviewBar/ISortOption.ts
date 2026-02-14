import type { SortDirection, MediaSortField } from '@enums';

export interface ISortOption {
	field: MediaSortField;
	label: string;
	direction: SortDirection;
}
