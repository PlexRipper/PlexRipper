export interface QTreeViewTableHeader {
	name: string;
	label: string;
	field: string;
	sortable?: boolean;
	required?: boolean;
	align: 'left' | 'center' | 'right';
}
