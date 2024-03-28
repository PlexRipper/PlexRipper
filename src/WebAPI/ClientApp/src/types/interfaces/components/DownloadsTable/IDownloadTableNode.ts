import type { TreeNode } from 'primevue/treenode/TreeNode.d.ts';
import { DownloadProgressDTO } from '@dto';

export interface IDownloadTableNode extends TreeNode, Omit<DownloadProgressDTO, 'children'> {
	children?: IDownloadTableNode[];
}
