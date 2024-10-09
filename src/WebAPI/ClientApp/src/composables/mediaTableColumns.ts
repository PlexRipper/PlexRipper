import type { QTreeViewTableHeader } from '@props';
import { useI18n } from '#build/imports';

export const getMediaTableColumns = (): QTreeViewTableHeader[] => {
	const { t } = useI18n();

	return	[
		{
			label: t('components.media-list.columns.index'),
			field: 'index',
			align: 'left',
			type: 'index',
			sortOrder: 'asc',
			sortable: true,
		},
		{
			label: t('components.media-list.columns.title'),
			field: 'title',
			sortField: 'sortIndex',
			type: 'title',
			align: 'left',
			sortOrder: 'asc',
			sortable: true,
			required: true,
		},
		{
			label: t('components.media-list.columns.year'),
			field: 'year',
			align: 'center',
			sortable: true,
		},
		{
			label: t('components.media-list.columns.duration'),
			field: 'duration',
			type: 'duration',
			align: 'center',
			sortable: true,
		},
		{
			label: t('components.media-list.columns.media-size'),
			field: 'mediaSize',
			type: 'file-size',
			align: 'center',
			sortable: true,
		},
		{
			label: t('components.media-list.columns.added-at'),
			field: 'addedAt',
			type: 'date',
			align: 'center',
			sortable: true,
		},
		{
			label: t('components.media-list.columns.updated-at'),
			field: 'updatedAt',
			type: 'date',
			align: 'center',
			sortable: true,
		},
		{
			label: t('components.media-list.columns.actions'),
			field: 'actions',
			align: 'center',
			type: 'actions',
			required: true,
		},
	];
};

export const getDownloadPreviewTableColumns = (): QTreeViewTableHeader[] => {
	const { t } = useI18n();
	return [
		{
			label: t('components.download-confirmation.columns.title'),
			field: 'title',
			align: 'left',
			sortOrder: 'asc',
			sortable: true,
			required: true,
		},
		{
			label: t('components.download-confirmation.columns.file-size'),
			field: 'size',
			type: 'file-size',
			width: 120,
			align: 'right',
		},
	];
};
