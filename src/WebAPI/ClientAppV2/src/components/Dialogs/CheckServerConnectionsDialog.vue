<template>
	<q-dialog v-model="visible" width="800" scrollable>
		<q-card class="account-setup-progress">
			<q-card-section>
				<!-- The total progress -->
				<progress-component
					:percentage="getTotalPercentage"
					:completed="getTotalPercentage === 100"
					:text="getProgressText"
					circular-mode
					:indeterminate="getMergedPlexServers.length === 0" />
			</q-card-section>

			<q-separator />

			<q-card-section>
				<q-list v-if="getMergedPlexServers.length > 0" expand class="server-progress-list">
					<template v-for="server in getMergedPlexServers">
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
									<q-item
										v-for="connection in server.plexServerConnections"
										v-if="server.plexServerConnections.length > 0">
										<!--	Plex Server Connection Status Icon -->
										<q-item-section avatar>
											<q-status
												:value="
													connection.progress
														? connection.progress.connectionSuccessful &&
														  connection.progress.completed
														: false
												" />
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
			</q-card-section>

			<q-card-actions align="right">
				<q-row justify="end">
					<q-col cols="auto">
						<HideButton @click="visible = false" />
					</q-col>
				</q-row>
			</q-card-actions>
		</q-card>
	</q-dialog>
</template>

<script setup lang="ts">
import Log from 'consola';
import { computed, ref } from 'vue';
import { clamp } from 'lodash-es';
import { useSubscription } from '@vueuse/rxjs';
import { filter, switchMap, tap } from 'rxjs/operators';
import type { JobStatusUpdateDTO, PlexAccountDTO, PlexServerDTO } from '@dto/mainApi';
import { JobStatus, JobTypes, ServerConnectionCheckStatusProgressDTO } from '@dto/mainApi';
import { AccountService, BackgroundJobsService, ServerService, SignalrService } from '@service';
import { useI18n } from '#imports';

const visible = ref(false);
const plexServers = ref<PlexServerDTO[]>([]);
const connectionProgress = ref<ServerConnectionCheckStatusProgressDTO[]>([]);
const account = ref<PlexAccountDTO | null>(null);
const t = useI18n().t;

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
	if (account.value) {
		return t('components.check-server-connections-progress.no-servers', {
			displayName: account.value.displayName,
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

const serverStatus = (plexServer: PlexServerDTO): boolean => {
	return plexServer.plexServerConnections.some(
		(x) => (x.progress?.connectionSuccessful ?? false) && (x.progress?.completed ?? false),
	);
};

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

onMounted(() => {
	useSubscription(
		BackgroundJobsService.getJobs(JobTypes.InspectPlexServerByPlexAccountIdJob)
			.pipe(
				tap((value) => Log.info('Background jobs fired', value)),
				filter((update) => update.status === JobStatus.Running),
				tap(() => (visible.value = true)),
				switchMap((update: JobStatusUpdateDTO) => AccountService.getAccount(update.primaryKeyValue)),
				tap((newAccount) => (account.value = newAccount)),
				switchMap(() => ServerService.refreshPlexServers()),
				switchMap(() => ServerService.getServersByPlexAccountId(account.value?.id ?? 0)),
			)
			.subscribe((servers) => {
				plexServers.value = servers;
			}),
	);

	useSubscription(
		SignalrService.getAllServerConnectionProgress().subscribe((connections) => {
			connectionProgress.value = connections;
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
