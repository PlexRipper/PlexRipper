export enum QTreeTableColumnType {
	Text = 'text',
	Title = 'title',
	FileSize = 'file-size',
	FileSpeed = 'file-speed',
	Duration = 'duration',
	Date = 'date',
	DateTime = 'datetime',
	Percentage = 'percentage',
	Actions = 'actions',
	Custom = 'custom',
}

export interface QTreeTableColumn {
	field: string;
	header: string;
	type?: QTreeTableColumnType;
	width?: number;
	align?: 'left' | 'center' | 'right';
	expander?: boolean;
	selection?: boolean;
	sortable?: boolean;
	headerStyle?: string;
	bodyStyle?: string;
}
