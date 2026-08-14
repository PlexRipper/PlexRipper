<template>
	<QCardDialog
		full-height
		:type="0 as number"
		:name="DialogType.SyncServerMediaDialog"
		cy="sync-server-media-dialog"
		@opened="onOpened"
		@closed="onClosed">
		<template #top-row>
			<!-- The total progress -->
			<ProgressComponent
				class="q-ma-md"
				circular-mode
				:percentage="totalPercentage"
				:completed="totalPercentage === 100"
				:text="getProgressText"
				:indeterminate="plexServerNodes.length === 0" />
		</template>
		<template #default>
			<div>
				<q-tree
					v-if="plexServerNodes.length > 0"
					v-model:expanded="expanded"
					:nodes="plexServerNodes"
					node-key="index"
					default-expand-all>
					<template #default-header="{ node }: { node: IPlexMediaSyncServerNode }">
						<QRow
							justify="between"
							class="q-mr-lg"
							align="center">
							<QCol
								cols="4">
								<div :class="{ 'text-weight-bold': isServer(node) }">
									<!--	Row Icon -->
									<q-icon
										v-if="isServer(node)"
										name="mdi-server"
										size="28px"
										class="q-mr-sm" />
									<QMediaTypeIcon
										v-else
										:active="node.completed"
										:loading="node.percentage > 0 && !node.completed"
										:media-type="node.mediaType" />
									<!-- Row Title	-->
									<span
										:class="[
											isServer(node)
												? 'sync-server-media-dialog-server-title'
												: 'sync-server-media-dialog-library-title',
											'q-ml-sm',
										]"
										:data-cy="
											isServer(node)
												? 'sync-server-media-dialog-server-title'
												: 'sync-server-media-dialog-library-title'
										">
										{{ node.title }}
									</span>
								</div>
							</QCol>
							<!-- Time Remaining -->
							<QCol
								cols="4"
								text-align="center">
								<QCountdown
									v-if="!isServer(node)"
									data-cy="sync-server-media-dialog-library-eta"
									:value="libraryStore.getLibraryProgress(node.id)?.timeRemaining ?? ''" />
							</QCol>
							<!-- Plex Media Sync Progress -->
							<QCol cols="4">
								<QProgressBar :value="node.percentage" />
							</QCol>
						</QRow>
					</template>
				</q-tree>
			</div>
		</template>
		<!-- Actions -->
		<template #actions="{ close }">
			<QRow justify="end">
				<QCol cols="auto">
					<HideButton
						cy="sync-server-media-dialog-hide-btn"
						@click="close" />
				</QCol>
			</QRow>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import type {
	LibrarySyncProgressDTO,
	PlexMediaType,
} from '@dto';
import { DialogType } from '@enums';
import { sum, meanBy } from 'lodash-es';
import {
	useI18n,
	useServerStore,
	useLibraryStore,
} from '#imports';

const { t } = useI18n();
const serverStore = useServerStore();
const libraryStore = useLibraryStore();

const expanded = ref<number[]>([]);
/**
 * When set, only progress for the server that initiated the sync is displayed.
 * A null value keeps the background activity dialog's all-server view.
 */
const plexServerId = ref<number | null>(null);

const libraryProgressList = computed(() => get(plexServerNodes).flatMap((x) => x.children).flatMap((x) => x.progress));

const totalPercentage = computed(() => {
	const nodes = get(plexServerNodes);
	if (nodes.length === 0) {
		return 0;
	}
	return sum(nodes.map((x) => x.percentage)) / nodes.length;
});

const plexServers = computed(() => serverStore.getServers(get(plexServerNodes).map((x) => x.id)));

const getProgressText = computed(() => {
	if (get(plexServers).length === 0) {
		return t('components.sync-server-media-dialog.fetching-servers', {
			displayName: t('general.error.unknown'),
		});
	}

	if (get(totalPercentage) === 100) {
		// Close all expanded nodes
		set(expanded, []);
		return t('components.sync-server-media-dialog.completed', {
			length: get(plexServers).length,
		});
	}

	return t('components.sync-server-media-dialog.checking-progress', {
		count: get(libraryProgressList).filter((x) => x?.isComplete).length,
		total: get(libraryProgressList).length,
	});
});

const plexServerNodes = computed((): IPlexMediaSyncServerNode[] => {
	let uniqueIndex = 0;

	return libraryStore
		.getLibrarySyncQueueGrouped(get(plexServerId) ?? undefined)
		.map((server) => {
			const percentage = server.progress.length > 0
				? meanBy(server.progress, (x) => x.percentage ?? 0)
				: 0;
			return {
				id: server.serverId,
				index: uniqueIndex++,
				type: 'server',
				title: serverStore.getServerName(server.serverId),
				percentage: percentage,
				completed: percentage === 100,
				children: server.progress.map((libraryProgress) => {
					const library = libraryStore.getLibrary(libraryProgress.plexLibraryId);
					return {
						id: libraryProgress.plexLibraryId,
						index: uniqueIndex++,
						type: 'library',
						mediaType: library?.type,
						percentage: libraryProgress.percentage ?? 0,
						title: library?.title ?? t('general.error.unknown'),
						completed: libraryProgress.isComplete ?? false,
						progress: libraryProgress,
						children: [],
					};
				}),
			};
		});
});

function isServer(node: IPlexMediaSyncServerNode): boolean {
	return node.type === 'server';
}

function onOpened(serverId?: number): void {
	set(plexServerId, serverId ?? null);
}

function onClosed(): void {
	set(plexServerId, null);
	set(expanded, []);
	libraryStore.clearCompletedSyncQueues();
}

interface IPlexMediaSyncServerNode {
	id: number;
	title: string;
	index: number;
	type: 'server' | 'library';
	percentage: number;
	completed: boolean;
	progress?: LibrarySyncProgressDTO;
	mediaType?: PlexMediaType;
	children: IPlexMediaSyncServerNode[];
}
</script>
