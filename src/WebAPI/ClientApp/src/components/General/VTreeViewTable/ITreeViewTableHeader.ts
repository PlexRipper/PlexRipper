import { DataTableHeader } from 'vuetify/types';
import TreeViewTableHeaderEnum from '@enums/treeViewTableHeaderEnum';
import ITreeViewTableAction from '@vTreeViewTable/ITreeViewTableAction';

export default interface ITreeViewTableHeader<T extends any = any> extends DataTableHeader<T> {
	type?: TreeViewTableHeaderEnum;
	actions?: ITreeViewTableAction[];
	maxWidth?: number;
}
