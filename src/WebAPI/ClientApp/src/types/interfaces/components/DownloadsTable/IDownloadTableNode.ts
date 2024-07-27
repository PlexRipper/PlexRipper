import type { TreeNode } from 'primevue/treenode';
import type { DownloadProgressDTO } from '@dto';

export interface IDownloadTableNode extends TreeNode, Omit<DownloadProgressDTO, 'children'> {
	children?: IDownloadTableNode[];
}
