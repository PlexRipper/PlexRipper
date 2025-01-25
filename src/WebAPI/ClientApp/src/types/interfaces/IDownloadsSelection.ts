import type { IPTreeTableSelectionKeys } from '@interfaces';

export interface IDownloadsSelection {
	plexServerId: number;
	maxSelectionCount: number;
	allSelection: IPTreeTableSelectionKeys;
	selection: IPTreeTableSelectionKeys;
}
