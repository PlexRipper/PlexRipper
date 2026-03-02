import type { TreeNode } from 'primevue/treenode';
import type { DownloadActions, DownloadProgressDTO } from '@dto';

export interface IDownloadTableNode extends Omit<TreeNode, 'key'>, Omit<DownloadProgressDTO, 'children'> {
	children?: IDownloadTableNode[];
	actions: {
		type: DownloadActions;
		loading: boolean;
		disabled: boolean;
	}[];
}
