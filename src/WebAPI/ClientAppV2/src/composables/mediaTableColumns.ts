import { QTreeViewTableHeader } from '@props';

export const getMediaTableColumns = (): QTreeViewTableHeader[] => {
	return [
		{
			label: 'Title',
			field: 'title',
			name: 'title',
			align: 'left',
			sortable: true,
			required: true,
		},
		{
			label: 'Year',
			name: 'year',
			field: 'year',
			align: 'left',
			sortable: true,
		},
		{
			label: 'Size',
			field: 'mediaSize',
			name: 'size',
			align: 'left',
			sortable: true,
		},
		{
			label: 'Added At',
			name: 'addedAt',
			field: 'addedAt',
			align: 'left',
			sortable: true,
		},
		{
			label: 'Updated At',
			name: 'updatedAt',
			field: 'updatedAt',
			align: 'left',
			sortable: true,
		},
		{
			label: 'Actions',
			name: 'actions',
			field: 'actions',
			align: 'left',
			required: true,
		},
	];
};
