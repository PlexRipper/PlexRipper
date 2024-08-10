<template>
	<QCardDialog
		max-width="1000px"
		content-height="80"
		:name="name"
		cy="check-server-connection-dialog"
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
			<q-tree
				v-if="plexServerNodes.length > 0"
				v-model:expanded="expanded"
				:nodes="plexServerNodes"
				node-key="index"
				default-expand-all>
				<template #default-header="{ node }: { node: IPlexServerNode }">
					<QRow
						justify="between"
						align="center">
						<QCol cols="8">
							<div :class="{ 'text-weight-bold': isServer(node) }">
								<!--	Plex Server Connection Icon -->
								<q-icon
									v-if="isServer(node)"
									name="mdi-server"
									size="28px"
									class="q-mr-sm" />
								<QConnectionIcon
									v-else
									:local="node?.local ?? false" />
								<!-- Plex Server Connection Url	-->
								<span
									:class="[
										isServer(node)
											? 'check-server-connections-dialog-server-title'
											: 'check-server-connections-dialog-connection-title',
										'q-ml-sm',
									]"
									:data-cy="
										isServer(node)
											? 'check-server-connections-dialog-server-title'
											: 'check-server-connections-dialog-connection-title'
									">
									{{ node.title }}
								</span>
							</div>
						</QCol>
						<QCol
							cols="4"
							:style="{ 'max-width': `600px !important` }">
							<QRow
								justify-end
								no-wrap
								justify="end">
								<!--	Plex Server Progress Status Icon -->
								<QCol cols="3">
									<QSpinnerRadio
										v-if="!node.completed"
										color="red"
										size="2em" />
									<QStatus
										v-else
										:value="node.connectionSuccessful" />
								</QCol>
								<!-- Plex Server Connection Progress	-->
								<QCol cols="9">
									<template v-if="isServer(node) && node.completed">
										<!-- No Plex Server Connection -->
										<span
											v-if="node.noConnections"
											:class="{ 'text-weight-bold': node.type === 'server' }">
											{{ t('components.check-server-connections-dialog.no-connections') }}
										</span>
										<span
											v-else-if="node.connectionSuccessful"
											:class="{ 'text-weight-bold': node.type === 'server' }">
											{{ t('components.check-server-connections-dialog.server-connectable') }}
										</span>
										<span
											v-else
											:class="{ 'text-weight-bold': node.type === 'server' }">
											{{ t('components.check-server-connections-dialog.server-un-connectable') }}
										</span>
									</template>
									<template v-else-if="node.progress">
										<ConnectionProgressText :progress="node.progress" />
									</template>
								</QCol>
							</QRow>
						</QCol>
					</QRow>
				</template>
			</q-tree>
		</template>
		<template #actions="{ close }">
			<QRow justify="end">
				<QCol cols="auto">
					<HideButton
						cy="check-server-connection-dialog-hide-btn"
						@click="close" />
				</QCol>
			</QRow>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { filter, tap } from 'rxjs/operators';
import { merge } from 'rxjs';
import { get, set } from '@vueuse/core';
import { JobTypes, JobStatus, type ServerConnectionCheckStatusProgressDTO, type JobStatusUpdateDTO } from '@dto';
import Log from 'consola';
import { useBackgroundJobsStore, useI18n, useOpenControlDialog, useServerStore, useSignalrStore } from '#imports';

const { t } = useI18n();
const serverStore = useServerStore();
const backgroundJobStore = useBackgroundJobsStore();
const name = 'checkServerConnectionDialogName';
const connectionProgress = ref<ServerConnectionCheckStatusProgressDTO[]>([]);

const expanded = ref<number[]>([]);
/**
 * The plex server ids that are being checked
 */
const plexServerIds = ref<number[]>([]);
const completedCount = computed(() => {
	return get(plexServerNodes).filter((progress) => progress.completed).length;
});

const totalPercentage = computed(() => {
	if (get(plexServerNodes).length === 0) {
		return 0;
	}
	return clamp(Math.round((get(completedCount) / get(plexServerNodes).length) * 100), 0, 100);
});
const plexServers = computed(() => serverStore.getServers([...get(connectionProgress).map((x) => x.plexServerId), ...get(plexServerIds)].filter((x, i, a) => a.indexOf(x) == i)));

const getProgressText = computed(() => {
	if (get(plexServers).length === 0) {
		return t('components.check-server-connections-dialog.fetching-servers', {
			displayName: t('general.error.unknown'),
		});
	}
	if (get(totalPercentage) === 100) {
		// Close all expanded nodes
		set(expanded, []);
		return t('components.check-server-connections-dialog.completed', {
			length: get(plexServers).length,
		});
	}
	return t('components.check-server-connections-dialog.checking-progress', {
		count: get(completedCount),
		total: get(plexServerNodes).length,
	});
});

const plexServerNodes = computed((): IPlexServerNode[] => {
	let uniqueIndex = 0;
	return get(plexServers).map((server) => {
		const serverResult: IPlexServerNode = {
			id: server.id,
			type: 'server',
			title: serverStore.getServerName(server.id),
			completed: false,
			index: uniqueIndex++,
			connectionSuccessful: false,
			noConnections: server.plexServerConnections.length === 0,
			children: server.plexServerConnections.map((connection) => {
				const progress = getConnectionProgress(connection.id, server.id);
				return {
					id: connection.id,
					type: 'connection',
					title: connection.url,
					local: connection.local,
					completed: progress.completed,
					index: uniqueIndex++,
					connectionSuccessful: progress.connectionSuccessful,
					progress,
					children: [],
				};
			}),
		};

		serverResult.completed = serverResult.children?.some((x) => x.completed) ?? true;
		serverResult.connectionSuccessful = serverResult.children?.some((connection) => connection.connectionSuccessful) ?? false;

		return serverResult;
	});
});

function getConnectionProgress(connectionId: number, serverId: number): ServerConnectionCheckStatusProgressDTO {
	return (
		get(connectionProgress).find((x) => x.plexServerConnectionId === connectionId) ?? {
			// Add default progress object
			plexServerConnectionId: connectionId,
			plexServerId: serverId,
			connectionSuccessful: false,
			completed: false,
			message: t('components.check-server-connections-dialog.no-progress-yet'),
			retryAttemptCount: 0,
			retryAttemptIndex: 0,
			statusCode: 0,
			timeToNextRetry: 0,
		}
	);
}

function isServer(node: IPlexServerNode): boolean {
	return node.type === 'server';
}

function onClosed(): void {
	set(connectionProgress, []);
	set(plexServerIds, []);
}

onMounted(() => {
	useSubscription(
		useSignalrStore()
			.getAllServerConnectionProgress()
			.subscribe((connections) => {
				for (const connection of connections) {
					const i = get(connectionProgress).findIndex((x) => x.plexServerConnectionId === connection.plexServerConnectionId);
					if (i === -1) {
						get(connectionProgress).push(connection);
					} else {
						get(connectionProgress)[i] = connection;
					}
				}
			}),
	);

	useSubscription(
		merge(
			backgroundJobStore.getJobStatusUpdate(JobTypes.InspectPlexServerJob),
			backgroundJobStore.getJobStatusUpdate(JobTypes.CheckPlexServerConnectionsJob))
			.pipe(
				filter((update: JobStatusUpdateDTO) => update.status === JobStatus.Started),
				tap((update) => {
					const ids: number[] = JSON.parse(update.primaryKeyValue);
					Log.debug('primaryKeyValue', ids);
					if (isArray(ids)) {
						get(plexServerIds).push(...ids);
					}
					if (isNumber(ids)) {
						get(plexServerIds).push(ids);
					}
				}),
				tap(() => useOpenControlDialog(name)),
			)
			.subscribe(),
	);
});

interface IPlexServerNode {
	id: number;
	title: string;
	index: number;
	type: 'server' | 'connection';
	completed: boolean;
	connectionSuccessful: boolean;
	progress?: ServerConnectionCheckStatusProgressDTO;
	noConnections?: boolean;
	local?: boolean;
	children: IPlexServerNode[];
}
</script>

<style lang="scss">
.server-progress-list {
	.v-list-item__content {
		padding: 0;
	}

	&.theme--dark {
		.server-title {
			color: white;
		}
	}
}
</style>
