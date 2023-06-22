export interface QTreeViewTableHeader {
	label: string;
	field: string;
	sortField?: string;
	width?: number;
	type?: 'title' | 'duration' | 'file-size' | 'file-speed' | 'date' | 'actions' | 'datetime' | 'percentage' | 'index';
	sortable?: boolean;
	required?: boolean;
	sortOrder?: 'asc' | 'desc' | 'no-sort';
	align?: 'left' | 'center' | 'right';
	headerStyle?: string;
}
