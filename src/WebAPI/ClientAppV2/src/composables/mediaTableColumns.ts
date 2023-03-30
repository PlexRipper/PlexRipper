import { QTreeViewTableHeader } from '@props';

export const getMediaTableColumns = (): QTreeViewTableHeader[] => {
	return [
		{
			label: 'Title',
			field: 'title',
			name: 'title',
			align: 'left',
			sortOrder: 'ad',
			sortable: true,
			required: true,
		},
		{
			label: 'Year',
			name: 'year',
			field: 'year',
			align: 'right',
			sortable: true,
		},
		{
			label: 'Size',
			field: 'mediaSize',
			name: 'size',
			align: 'right',
			sortable: true,
		},
		{
			label: 'Added At',
			name: 'addedAt',
			field: 'addedAt',
			align: 'right',
			sortable: true,
		},
		{
			label: 'Updated At',
			name: 'updatedAt',
			field: 'updatedAt',
			align: 'right',
			sortable: true,
		},
		{
			label: 'Actions',
			name: 'actions',
			field: 'actions',
			align: 'right',
			required: true,
		},
	];
};
