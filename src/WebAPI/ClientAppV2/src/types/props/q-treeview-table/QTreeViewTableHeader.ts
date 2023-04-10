export interface QTreeViewTableHeader {
	name: string;
	label: string;
	field: string;
	width?: number;
	type?: 'duration' | 'file-size' | 'file-speed' | 'date' | 'actions' | 'datetime' | 'percentage';
	sortable?: boolean;
	required?: boolean;
	sortOrder?: 'ad' | 'da';
	align?: 'left' | 'center' | 'right';
	headerStyle?: string;
}
