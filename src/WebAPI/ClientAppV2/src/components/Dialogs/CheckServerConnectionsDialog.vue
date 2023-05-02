<template>
	<q-card-dialog max-width="1000px" :name="name">
		<template #top-row>
			<!-- The total progress -->
			<progress-component
				:percentage="getTotalPercentage"
				:completed="getTotalPercentage === 100"
				:text="getProgressText"
				circular-mode
				:indeterminate="getMergedPlexServers.length === 0" />
		</template>
		<template #default>
			<q-list v-if="getMergedPlexServers.length > 0" expand class="server-progress-list">
				<template v-for="server in getMergedPlexServers" :key="server.id">
					<!--	Plex Server -->
					<q-expansion-item expand-separator :model-value="false">
						<!--	Plex Server Title -->
						<template #header>
							<q-item-section avatar>
								<!--	Plex Server Progress Icon -->
								<boolean-progress
									v-if="!getServerProgress(server).completed"
									:loading="!getServerProgress(server).completed"
									:success="getServerProgress(server).connectionSuccessful" />
								<q-status v-else :value="serverStatus(server)" />
							</q-item-section>

							<q-item-section>
								<span class="server-title">
									{{ server.name }}
								</span>
							</q-item-section>

							<q-item-section side>
								<ConnectionProgressText :progress="getServerProgress(server)" />
							</q-item-section>
						</template>
						<template #default>
							<q-list>
								<template v-if="server.plexServerConnections.length > 0">
									<q-item v-for="connection in server.plexServerConnections" :key="connection.id">
										<!--	Plex Server Connection Status Icon -->
										<q-item-section avatar>
											<q-status :value="isConnectionStatus(connection)" />
										</q-item-section>
										<!-- Plex Server Connection Url	-->
										<q-item-section>
											{{ connection.url }}
										</q-item-section>
										<!-- Plex Server Connection Progress	-->
										<q-item-section>
											<ConnectionProgressText :progress="connection.progress" />
										</q-item-section>
										<!--	Plex Server Progress Status Icon -->
										<q-item-section>
											<boolean-progress
												:loading="!connection.progress.completed"
												:success="connection.progress.connectionSuccessful" />
										</q-item-section>
									</q-item>
								</template>

								<!-- No Plex Server Connection -->
								<q-item v-else>
									<q-item-section>
										<q-item-label>No Connections found for this Plex server!</q-item-label>
									</q-item-section>
								</q-item>
							</q-list>
						</template>
					</q-expansion-item>
				</template>
			</q-list>

			<!-- No Server Warning	-->
			<q-row v-else justify="center">
				<q-col cols="auto">
					<h2>
						{{ noServerWarning }}
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
import Log from 'consola';
import { computed, ref, onMounted } from 'vue';
import { clamp } from 'lodash-es';
import { useSubscription } from '@vueuse/rxjs';
import { filter, switchMap, tap } from 'rxjs/operators';
import { get, set } from '@vueuse/core';
import type { JobStatusUpdateDTO, PlexAccountDTO, PlexServerConnectionDTO, PlexServerDTO } from '@dto/mainApi';
import { JobStatus, JobTypes, ServerConnectionCheckStatusProgressDTO } from '@dto/mainApi';
import { AccountService, BackgroundJobsService, ServerService, SignalrService } from '@service';
import { useI18n, useOpenControlDialog } from '#imports';

const { t } = useI18n();
const name = 'checkServerConnectionDialogName';
const plexServers = ref<PlexServerDTO[]>([]);
const connectionProgress = ref<ServerConnectionCheckStatusProgressDTO[]>([]);
const account = ref<PlexAccountDTO | null>(null);

const getCompletedCount = computed(() => {
	return connectionProgress.value.filter((progress) => progress.completed).length;
});

const getTotalPercentage = computed(() => {
	if (connectionProgress.value.length === 0) {
		return 0;
	}
	return clamp(Math.round((getCompletedCount.value / connectionProgress.value.length) * 100), 0, 100);
});

const noServerWarning = computed(() => {
	if (get(account)) {
		return t('components.check-server-connections-progress.no-servers', {
			displayName: get(account)?.displayName ?? '',
		});
	}
	return t('components.check-server-connections-progress.no-servers-no-account');
});

const getProgressText = computed(() => {
	if (plexServers.value.length === 0) {
		return `Retrieving accessible servers for account ${account.value?.displayName ?? ''}.`;
	}
	if (getTotalPercentage.value === 100) {
		return t('components.check-server-connections-progress.completed', {
			length: plexServers.value.length,
		});
	}
	return t('components.check-server-connections-progress.checking', {
		completed: getCompletedCount.value,
		total: getMergedPlexServers.value.length,
	});
});

const getMergedPlexServers = computed(() => {
	return plexServers.value.map((server) => {
		return {
			...server,
			plexServerConnections: server.plexServerConnections.map((connection) => {
				return {
					...connection,
					progress: connectionProgress.value.find((x) => x.plexServerConnectionId === connection.id) ?? {
						// Add default progress object
						plexServerConnectionId: connection.id,
						plexServerId: connection.plexServerId,
						connectionSuccessful: false,
						completed: false,
						message: 'No progress yet',
						retryAttemptCount: 0,
						retryAttemptIndex: 0,
						statusCode: 0,
						timeToNextRetry: 0,
					},
				};
			}),
		};
	});
});

function serverStatus(plexServer: PlexServerDTO): boolean {
	return plexServer.plexServerConnections.some(
		(x) => (x.progress?.connectionSuccessful ?? false) && (x.progress?.completed ?? false),
	);
}

function isConnectionStatus(connection: PlexServerConnectionDTO) {
	return connection.progress ? connection.progress.connectionSuccessful && connection.progress.completed : false;
}

function getServerProgress(plexServer: PlexServerDTO): ServerConnectionCheckStatusProgressDTO {
	return {
		completed: plexServer.plexServerConnections.every((x) => x.progress?.completed ?? false),
		connectionSuccessful: serverStatus(plexServer),
		plexServerId: plexServer.id,
		statusCode: 0,
		message: '',
		retryAttemptCount: 0,
		retryAttemptIndex: 0,
		timeToNextRetry: 0,
		plexServerConnectionId: 0,
	};
}

onMounted(() => {
	Log.info('Check server connections dialog mounted');
	useSubscription(
		BackgroundJobsService.getJobs(JobTypes.InspectPlexServerByPlexAccountIdJob)
			.pipe(
				tap((value) => Log.info('Background jobs fired', value)),
				filter((update) => update.status === JobStatus.Running),
				tap(() => useOpenControlDialog(name)),
				switchMap((update: JobStatusUpdateDTO) => AccountService.getAccount(update.primaryKeyValue)),
				tap((newAccount) => set(account, newAccount)),
				switchMap(() => ServerService.refreshPlexServers()),
				switchMap(() => ServerService.getServersByPlexAccountId(account.value?.id ?? 0)),
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
