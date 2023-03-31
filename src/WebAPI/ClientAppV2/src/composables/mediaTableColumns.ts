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
			align: 'center',
			sortable: true,
		},
		{
			label: 'Size',
			field: 'mediaSize',
			name: 'size',
			align: 'center',
			sortable: true,
		},
		{
			label: 'Added At',
			name: 'addedAt',
			field: 'addedAt',
			align: 'center',
			sortable: true,
		},
		{
			label: 'Updated At',
			name: 'updatedAt',
			field: 'updatedAt',
			align: 'center',
			sortable: true,
		},
		{
			label: 'Actions',
			name: 'actions',
			field: 'actions',
			align: 'center',
			required: true,
		},
	];
};
