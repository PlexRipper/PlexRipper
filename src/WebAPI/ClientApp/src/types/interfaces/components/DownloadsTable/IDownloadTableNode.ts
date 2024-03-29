import { type TreeNode } from 'primevue/tree/Tree';
import { DownloadProgressDTO } from '@dto';

export interface IDownloadTableNode extends TreeNode, Omit<DownloadProgressDTO, 'children'> {
	children?: IDownloadTableNode[];
}
