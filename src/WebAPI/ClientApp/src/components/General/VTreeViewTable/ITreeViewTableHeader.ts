import { DataTableHeader } from 'vuetify/types';
import TreeViewTableHeaderEnum from '@enums/treeViewTableHeaderEnum';

export default interface ITreeViewTableHeader<T extends any = any> extends DataTableHeader<T> {
	type?: TreeViewTableHeaderEnum;
	maxWidth?: number;
}
