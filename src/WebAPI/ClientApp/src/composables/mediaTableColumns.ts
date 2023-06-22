import { QTreeViewTableHeader } from '@props';

export const getMediaTableColumns = (): QTreeViewTableHeader[] => {
	return [
		{
			label: '#',
			field: 'index',
			align: 'left',
			type: 'index',
			sortOrder: 'asc',
			sortable: true,
		},
		{
			label: 'Title',
			field: 'title',
			// The media is pre sorted with the index, using the index is more efficient to than sort on and ensures the same sorting is used as the back-end uses
			sortField: 'index',
			type: 'title',
			align: 'left',
			sortOrder: 'asc',
			sortable: true,
			required: true,
		},
		{
			label: 'Year',
			field: 'year',
			align: 'center',
			sortable: true,
		},
		{
			label: 'Duration',
			field: 'duration',
			type: 'duration',
			align: 'center',
			sortable: true,
		},
		{
			label: 'Size',
			field: 'mediaSize',
			type: 'file-size',
			align: 'center',
			sortable: true,
		},
		{
			label: 'Added At',
			field: 'addedAt',
			type: 'date',
			align: 'center',
			sortable: true,
		},
		{
			label: 'Updated At',
			field: 'updatedAt',
			type: 'date',
			align: 'center',
			sortable: true,
		},
		{
			label: 'Actions',
			field: 'actions',
			align: 'center',
			type: 'actions',
			required: true,
		},
	];
};

export const getDownloadTableColumns: QTreeViewTableHeader[] = [
	{
		label: 'Title',
		field: 'title',
	},
	{
		label: 'Status',
		field: 'status',
		align: 'right',
		width: 120,
	},
	{
		label: 'Received',
		field: 'dataReceived',
		type: 'file-size',
		align: 'right',
		width: 120,
	},
	{
		label: 'Size',
		field: 'dataTotal',
		type: 'file-size',
		width: 120,
		align: 'right',
	},
	{
		label: 'Speed',
		field: 'downloadSpeed',
		type: 'file-speed',
		align: 'right',
		width: 120,
	},
	{
		label: 'ETA',
		field: 'timeRemaining',
		type: 'duration',
		align: 'right',
		width: 120,
	},
	{
		label: 'Percentage',
		field: 'percentage',
		type: 'percentage',
		align: 'center',
		width: 120,
	},
	{
		label: 'Actions',
		field: 'actions',
		type: 'actions',
		width: 200,
		align: 'center',
		sortable: false,
	},
];

export const getDownloadPreviewTableColumns: QTreeViewTableHeader[] = [
	{
		label: 'Title',
		field: 'title',
		align: 'left',
		sortOrder: 'asc',
		sortable: true,
		required: true,
	},
	{
		label: 'Size',
		field: 'size',
		type: 'file-size',
		width: 120,
		align: 'right',
	},
];
