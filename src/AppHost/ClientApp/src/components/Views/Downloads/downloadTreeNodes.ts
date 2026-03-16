import type { DownloadActions, DownloadProgressDTO } from '@dto';
import type { IDownloadTableNode } from '@interfaces';
import { toDownloadActions } from '@composables';

export interface DownloadTreeNodeLoadingState {
	id: string;
	action: DownloadActions;
}

export function toDownloadTreeNodes(
	value: DownloadProgressDTO[] | undefined,
	loadingIds: DownloadTreeNodeLoadingState[],
): IDownloadTableNode[] {
	return value?.map((node) => ({
		...node,
		key: node.id,
		children: toDownloadTreeNodes(node.children, loadingIds),
		actions: toDownloadActions(node.status).map((action) => ({
			type: action,
			loading: loadingIds.some((item) => item.id === node.id && item.action === action),
			disabled: loadingIds.some((item) => item.id === node.id && item.action !== action),
		})),
	})) ?? [];
}
