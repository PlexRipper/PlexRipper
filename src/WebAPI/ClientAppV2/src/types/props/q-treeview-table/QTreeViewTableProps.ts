import { QTreeViewTableItem } from './QTreeViewTableItem';
import { QTreeViewTableHeader } from './QTreeViewTableHeader';

export interface QTreeViewTableProps {
	nodes: QTreeViewTableItem[];

	columns: QTreeViewTableHeader[];
	labelKey?: string;
}
