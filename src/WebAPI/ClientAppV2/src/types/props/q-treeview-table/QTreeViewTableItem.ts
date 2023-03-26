export interface QTreeViewTableItem {
	id: number;
	key: number | string;
	title: string;
	childCount: number;
	children?: QTreeViewTableItem[];
}
