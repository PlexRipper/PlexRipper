<template>
	<v-dialog v-model="visible" width="800" scrollable>
		<v-card class="account-setup-progress">
			<v-card-title>
				<!-- The total progress -->
				<progress-component
					:percentage="getTotalPercentage"
					:completed="getTotalPercentage === 100"
					:text="getProgressText"
					circular-mode
					:indeterminate="getMergedPlexServers.length === 0"
				/>
			</v-card-title>
			<v-divider></v-divider>
			<vue-scroll>
				<v-card-text>
					<v-list v-if="getMergedPlexServers.length > 0" expand class="server-progress-list">
						<template v-for="(server, index) in getMergedPlexServers">
							<!--	Plex Server -->
							<v-list-group :key="`server-${index}`" :value="false">
								<template #activator>
									<v-list-item-content>
										<!--	Plex Server Title -->
										<v-list-item-title>
											<v-row>
												<v-col cols="auto">
													<!--	Plex Server Progress Icon -->
													<boolean-progress
														v-if="!serverProgress(server).completed"
														:loading="!serverProgress(server).completed"
														:success="serverProgress(server).connectionSuccessful"
													/>
													<status v-else :value="serverStatus(server)" />
												</v-col>
												<v-col cols="4">
													<span class="server-title">
														{{ server.name }}
													</span>
													<v-col cols="4">
														<ConnectionProgressText :progress="serverProgress(server)" />
													</v-col>
												</v-col>
											</v-row>
										</v-list-item-title>
									</v-list-item-content>
								</template>

								<!-- Plex Server Connection Details	-->
								<template v-if="server.plexServerConnections.length > 0">
									<v-list-item
										v-for="(connection, index2) in server.plexServerConnections"
										:key="index2"
										class="ml-4"
									>
										<v-list-item-content>
											<!--	Plex Server Connection Status Icon -->
											<v-list-item-title>
												<v-row>
													<v-col cols="auto">
														<status
															:value="
																connection.progress
																	? connection.progress.connectionSuccessful &&
																	  connection.progress.completed
																	: false
															"
														/>
													</v-col>
													<v-col cols="4">
														{{ connection.url }}
													</v-col>
													<v-col cols="4">
														<ConnectionProgressText :progress="connection.progress" />
													</v-col>
												</v-row>
											</v-list-item-title>
										</v-list-item-content>
										<!--	Plex Server Progress Status Icon -->
										<v-list-item-icon>
											<boolean-progress
												:loading="!connection.progress.completed"
												:success="connection.progress.connectionSuccessful"
											/>
										</v-list-item-icon>
									</v-list-item>
								</template>

								<v-list-item v-else>
									<v-list-item-title> No Connections found for this Plex server!</v-list-item-title>
								</v-list-item>
							</v-list-group>
							<v-divider :key="`hr-${index}`" />
						</template>
					</v-list>

					<!-- No Server Warning	-->
					<v-row v-else justify="center">
						<v-col cols="auto">
							<h2>
								{{ noServerWarning }}
							</h2>
						</v-col>
					</v-row>
				</v-card-text>
			</vue-scroll>
			<v-card-actions>
				<v-row justify="end">
					<v-col cols="auto">
						<HideButton @click="visible = false" />
					</v-col>
				</v-row>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import { clamp } from 'lodash-es';
import { useSubscription } from '@vueuse/rxjs';
import { filter, switchMap, tap } from 'rxjs/operators';
import type { JobStatusUpdateDTO, PlexAccountDTO, PlexServerDTO } from '@dto/mainApi';
import { JobStatus, JobTypes, ServerConnectionCheckStatusProgressDTO } from '@dto/mainApi';
import { generatePlexServers, MockConfig } from '@mock';
import { AccountService, BackgroundJobsService, ServerService, SignalrService } from '@service';

@Component
export default class CheckServerConnectionsProgress extends Vue {
	visible: boolean = false;

	plexServers: PlexServerDTO[] = [];

	connectionProgress: ServerConnectionCheckStatusProgressDTO[] = [];

	account: PlexAccountDTO | null = null;

	get getProgressText(): string {
		if (this.plexServers.length === 0) {
			return `Retrieving accessible servers for account ${this.account?.displayName ?? ''}.`;
		}
		if (this.getTotalPercentage === 100) {
			return this.$ts('components.check-server-connections-progress.completed', {
				length: this.plexServers.length,
			});
		}
		return this.$ts('components.check-server-connections-progress.checking', {
			completed: this.getCompletedCount,
			total: this.getMergedPlexServers.length,
		});
	}

	get getCompletedCount(): number {
		return this.getMergedPlexServers.filter((x) => x.plexServerConnections.every((y) => y.progress?.completed ?? false))
			.length;
	}

	get getTotalPercentage(): number {
		return clamp((this.getCompletedCount / this.plexServers.length) * 100, 0, 100);
	}

	get noServerWarning(): string {
		return this.$t('components.account-setup-progress.no-server-warning', {
			displayName: this.account ? this.account.displayName : this.$t('general.commands.unknown'),
		}).toString();
	}

	serverStatus(plexServer: PlexServerDTO): boolean {
		return plexServer.plexServerConnections.some(
			(x) => (x.progress?.connectionSuccessful ?? false) && (x.progress?.completed ?? false),
		);
	}

	serverProgress(plexServer: PlexServerDTO): ServerConnectionCheckStatusProgressDTO {
		return {
			completed: plexServer.plexServerConnections.every((x) => x.progress?.completed ?? false),
			connectionSuccessful: this.serverStatus(plexServer),
			plexServerId: plexServer.id,
			statusCode: 0,
			message: '',
			retryAttemptCount: 0,
			retryAttemptIndex: 0,
			timeToNextRetry: 0,
			plexServerConnectionId: 0,
		};
	}

	get getMergedPlexServers(): PlexServerDTO[] {
		return this.plexServers.map((server) => {
			return {
				...server,
				plexServerConnections: server.plexServerConnections.map((connection) => {
					return {
						...connection,
						progress: this.connectionProgress.find((x) => x.plexServerConnectionId === connection.id) ?? {
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
	}

	mounted(): void {
		const xxx = false;
		if (xxx) {
			const config: Partial<MockConfig> = {
				seed: 267398,
				plexAccountCount: 2,
				plexServerCount: 5,
				connectionHasProgress: true,
			};
			this.plexServers = generatePlexServers(config);

			for (const plexServer of this.plexServers) {
				for (const connection of plexServer.plexServerConnections) {
					if (connection.progress) {
						this.connectionProgress.push(connection.progress);
					}
				}
			}
		} else {
			useSubscription(
				BackgroundJobsService.getJobs(JobTypes.InspectPlexServerByPlexAccountIdJob)
					.pipe(
						tap((value) => Log.info('Background jobs fired', value)),
						filter((update) => update.status === JobStatus.Running),
						tap(() => (this.visible = true)),
						switchMap((update: JobStatusUpdateDTO) => AccountService.getAccount(update.primaryKeyValue)),
						tap((account) => (this.account = account)),
						switchMap(() => ServerService.refreshPlexServers()),
						switchMap(() => ServerService.getServersByPlexAccountId(this.account?.id ?? 0)),
					)
					.subscribe((servers) => {
						this.plexServers = servers;
					}),
			);

			useSubscription(
				SignalrService.getAllServerConnectionProgress().subscribe((connections) => {
					this.connectionProgress = connections;
				}),
			);
		}
	}
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
