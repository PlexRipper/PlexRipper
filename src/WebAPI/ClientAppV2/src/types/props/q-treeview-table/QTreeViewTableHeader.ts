export interface QTreeViewTableHeader {
	name: string;
	label: string;
	field: string;
	width?: number;
	sortable?: boolean;
	required?: boolean;
	sortOrder?: 'ad' | 'da';
	align?: 'left' | 'center' | 'right';
	headerStyle?: string;
}
