import type IPTreeTableSelectionKeys from '@interfaces/IPTreeTableSelectionKeys';

export default interface IDownloadsSelection {
	plexServerId: number;
	maxSelectionCount: number;
	allSelection: IPTreeTableSelectionKeys;
	selection: IPTreeTableSelectionKeys;
}
