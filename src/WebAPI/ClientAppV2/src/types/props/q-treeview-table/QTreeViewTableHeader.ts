export interface QTreeViewTableHeader {
	name: string;
	label: string;
	field: string;
	sortable?: boolean;
	required?: boolean;
	sortOrder?: 'ad' | 'da';
	align: 'left' | 'center' | 'right';
}
