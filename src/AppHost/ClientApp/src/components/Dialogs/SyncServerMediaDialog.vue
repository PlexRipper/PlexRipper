<template>
	<QCardDialog
		full-height
		:name="DialogType.SyncServerMediaDialog"
		cy="sync-server-media-dialog"
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
							<!-- Progress Bar -->
							<QCol
								cols="8"
								:style="{ 'max-width': `600px !important` }">
								<QRow
									no-wrap
									justify="end">
									<!-- Steps Progress -->
									<QCol>
										<QText
											v-if="!isServer(node) && !node.completed"
											:value="$t('components.media-overview.steps-remaining', {
												index: node.progress?.step,
												total: node.progress?.totalSteps,
											})"
											align="center" />
									</QCol>
									<!-- Time Remaining -->
									<QCol>
										<QCountdown
											v-if="!node.completed"
											:value="node.progress?.timeRemaining ?? ''" />
									</QCol>
									<!--	Plex Media Sync Progress -->
									<QCol
										style="max-width: 300px;">
										<QProgressBar :value="node.percentage" />
									</QCol>
								</QRow>
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
	LibraryProgress,
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
 * The plex server ids that are being checked
 */
const plexServerIds = ref<number[]>([]);

const libraryProgressList = computed(() => get(plexServerNodes).flatMap((x) => x.progress));

const totalPercentage = computed(() => {
	return sum(get(plexServerNodes).map((x) => x.percentage)) / get(plexServerNodes).length;
});

const plexServers = computed(() => serverStore.getServers([...get(plexServerNodes).map((x) => x.id), ...get(plexServerIds)].filter((x, i, a) => a.indexOf(x) == i)));

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

	return libraryStore.getLibrarySyncQueueGrouped().map((server) => {
		const percentage = meanBy(server.progress, (x) => x.percentage);
		return {
			id: server.serverId,
			index: uniqueIndex++,
			type: 'server',
			title: serverStore.getServerName(server.serverId),
			percentage: percentage,
			completed: percentage === 100,
			children: server.progress.map((libraryProgress) => {
				const library = libraryStore.getLibrary(libraryProgress.id);
				return {
					id: libraryProgress.id,
					index: uniqueIndex++,
					type: 'library',
					mediaType: library?.type,
					percentage: libraryProgress.percentage,
					title: library?.title ?? t('general.error.unknown'),
					completed: libraryProgress.isComplete,
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

function onClosed(): void {
	set(plexServerIds, []);
}

interface IPlexMediaSyncServerNode {
	id: number;
	title: string;
	index: number;
	type: 'server' | 'library';
	percentage: number;
	completed: boolean;
	progress?: LibraryProgress;
	mediaType?: PlexMediaType;
	children: IPlexMediaSyncServerNode[];
}
</script>
