import TreeViewTableHeaderEnum from '@enums/treeViewTableHeaderEnum';

export default interface ITreeViewTableHeader<T extends any = any> {
	type?: TreeViewTableHeaderEnum;
	maxWidth?: number;
	defaultActions?: string[];
}
