<template>
	<q-card-dialog max-width="1000px" full-height :name="name">
		<template #top-row>
			<!-- The total progress -->
			<progress-component
				class="q-ma-md"
				circular-mode
				:percentage="totalPercentage"
				:completed="totalPercentage === 100"
				:text="getProgressText"
				:indeterminate="plexServerNodes.length === 0" />
		</template>
		<template #default>
			<q-tree v-if="plexServerNodes.length > 0" :nodes="plexServerNodes" node-key="title" default-expand-all>
				<template #default-header="{ node }: { node: IPlexServerNode }">
					<q-row justify="between" align="center">
						<q-col cols="4">
							<div :class="{ 'text-weight-bold': node.type === 'server' }">
								<!--	Plex Server Connection Icon -->
								<q-icon v-if="isServer(node)" name="mdi-server" size="28px" class="q-mr-sm" />
								<QConnectionIcon v-else :local="node?.local ?? false" />
								<!-- Plex Server Connection Url	-->
								<span class="q-ml-sm">
									{{ node.title }}
								</span>
							</div>
						</q-col>
						<q-col cols="8" :style="{ 'max-width': `600px !important` }">
							<q-row justify-end no-wrap justify="end">
								<!--	Plex Server Progress Status Icon -->
								<q-col cols="3">
									<q-spinner-radio v-if="!node.completed" color="red" size="2em" />
									<q-status v-else :value="node.connectionSuccessful" />
								</q-col>
								<!-- Plex Server Connection Progress	-->
								<q-col cols="9">
									<template v-if="isServer(node) && node.completed">
										<!-- No Plex Server Connection -->
										<span v-if="node.noConnections">
											{{ $t('components.check-server-connections-dialog.no-connections') }}
										</span>
										<span v-else-if="node.connectionSuccessful">
											{{ $t('components.check-server-connections-dialog.server-connectable') }}
										</span>
										<span v-else>
											{{ $t('components.check-server-connections-dialog.server-un-connectable') }}
										</span>
									</template>
									<template v-else-if="node.progress">
										<ConnectionProgressText :progress="node.progress" />
									</template>
								</q-col>
							</q-row>
						</q-col>
					</q-row>
				</template>
			</q-tree>
			<!-- No Server Warning	-->
			<q-row v-else justify="center">
				<q-col cols="auto">
					<h2>
						{{
							$t('components.check-server-connections-dialog.no-servers', {
								displayName: account?.displayName ?? $t('general.error.unknown'),
							})
						}}
					</h2>
				</q-col>
			</q-row>
		</template>
		<template #actions="{ close }">
			<q-row justify="end">
				<q-col cols="auto">
					<HideButton @click="close" />
				</q-col>
			</q-row>
		</template>
	</q-card-dialog>
</template>

<script setup lang="ts">
import { computed, ref, onMounted } from 'vue';
import { clamp } from 'lodash-es';
import { useSubscription } from '@vueuse/rxjs';
import { filter, switchMap, tap } from 'rxjs/operators';
import { get, set } from '@vueuse/core';
import type { JobStatusUpdateDTO, PlexAccountDTO, PlexServerDTO } from '@dto/mainApi';
import { JobStatus, JobTypes, ServerConnectionCheckStatusProgressDTO } from '@dto/mainApi';
import { AccountService, BackgroundJobsService, ServerService, SignalrService } from '@service';
import { useI18n, useOpenControlDialog } from '#imports';

const { t } = useI18n();
const name = 'checkServerConnectionDialogName';
const plexServers = ref<PlexServerDTO[]>([]);
const connectionProgress = ref<ServerConnectionCheckStatusProgressDTO[]>([]);
const account = ref<PlexAccountDTO | null>(null);

const completedCount = computed(() => {
	return get(plexServerNodes).filter((progress) => progress.completed).length;
});

const totalPercentage = computed(() => {
	if (get(plexServerNodes).length === 0) {
		return 0;
	}
	return clamp(Math.round((get(completedCount) / get(plexServerNodes).length) * 100), 0, 100);
});

const getProgressText = computed(() => {
	if (get(plexServers).length === 0) {
		return t('components.check-server-connections-dialog.fetching-servers', {
			displayName: get(account)?.displayName ?? t('general.error.unknown'),
		});
	}
	if (get(totalPercentage) === 100) {
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
	return get(plexServers).map((server) => {
		const serverResult: IPlexServerNode = {
			id: server.id,
			type: 'server',
			title: server.name,
			completed: false,
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
			message: 'No progress yet',
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

onMounted(() => {
	useSubscription(
		BackgroundJobsService.getJobs(JobTypes.InspectPlexServerByPlexAccountIdJob)
			.pipe(
				filter((update) => update.status === JobStatus.Running),
				tap(() => useOpenControlDialog(name)),
				switchMap((update: JobStatusUpdateDTO) => AccountService.getAccount(update.primaryKeyValue)),
				tap((newAccount) => set(account, newAccount)),
				switchMap(() => ServerService.refreshPlexServers()),
				switchMap(() => ServerService.getServersByPlexAccountId(get(account)?.id ?? 0)),
			)
			.subscribe((servers) => {
				set(plexServers, servers);
			}),
	);

	useSubscription(
		SignalrService.getAllServerConnectionProgress().subscribe((connections) => {
			set(connectionProgress, connections);
		}),
	);
});

interface IPlexServerNode {
	id: number;
	title: string;
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
