<template>
	<v-card>
		<v-card-text>
			<!-- The total progress -->
			<progress-component
				:percentage="getTotalPercentage"
				:completed="getTotalPercentage === 100"
				:text="getProgressText"
				circular-mode
				:indeterminate="plexServers.length === 0"
			/>
			<!--	Server Connection Details	-->
			<v-simple-table v-if="getServerUpdates.length > 0" class="section-table">
				<tbody>
					<tr v-for="{ server, progress } in getServerUpdates" :key="server.id">
						<!--	Server name and status	-->
						<td style="width: 30%">
							<status :value="progress ? progress.connectionSuccessful && progress.completed : false" />
							{{ server.name }}
						</td>
						<!--	Status icon	-->
						<td style="width: 10%">
							<template v-if="progress">
								<template v-if="!progress.completed">
									<v-progress-circular indeterminate color="red" />
								</template>
								<template v-else>
									<v-icon v-if="progress.connectionSuccessful">mdi-check</v-icon>
									<v-icon v-else>mdi-close</v-icon>
								</template>
							</template>
						</td>
						<!--	Current Action	-->
						<td style="width: 30%">
							<template v-if="progress">
								<template v-if="!progress.completed">
									<span v-if="progress && progress.retryAttemptIndex > 0">
										{{
											$t('components.account-setup-progress.retry-connection', {
												attemptIndex: progress.retryAttemptIndex,
												attemptCount: progress.retryAttemptCount,
											})
										}}
									</span>
								</template>
								<!--	Completed -->
								<template v-else>
									<span v-if="progress.connectionSuccessful">
										{{ $t('components.account-setup-progress.server-connectable') }}
									</span>
									<span v-else>
										{{ $t('components.account-setup-progress.server-un-connectable') }}
									</span>
								</template>
							</template>
						</td>
						<!--	Error message	-->
						<td style="width: 30%">
							<template v-if="progress">
								<span v-if="progress && progress.message">
									{{ progress.message }}
								</span>
							</template>
						</td>
					</tr>
				</tbody>
			</v-simple-table>
			<!-- No Server Warning	-->
			<v-row v-else justify="center">
				<v-col cols="auto">
					<h2>
						{{
							$t('components.account-setup-progress.no-server-warning', {
								displayName: account ? account.displayName : 'unknown',
							})
						}}
					</h2>
				</v-col>
			</v-row>
		</v-card-text>
		<v-card-actions>
			<v-row justify="end">
				<v-col cols="auto">
					<p-btn :button-type="hideButtonType" @click="$emit('hide')" />
				</v-col>
			</v-row>
		</v-card-actions>
	</v-card>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import { clamp } from 'lodash-es';
import { SignalrService } from '@service';
import type { InspectServerProgress, PlexAccountDTO, PlexServerDTO } from '@dto/mainApi';
import ButtonType from '@enums/buttonType';

declare interface ServerUpdate {
	id: number;
	server: PlexServerDTO;
	progress: InspectServerProgress | null;
}

@Component
export default class AccountSetupProgress extends Vue {
	@Prop({ required: true, type: Object as () => PlexAccountDTO })
	readonly account!: PlexAccountDTO;

	inspectServerProgresses: InspectServerProgress[] = [];

	hideButtonType: ButtonType = ButtonType.Hide;
	confirmButtonType: ButtonType = ButtonType.Confirm;

	get plexServers(): PlexServerDTO[] {
		return this.account.plexServers ?? [];
	}

	get getServerUpdates(): ServerUpdate[] {
		const serverUpdates: ServerUpdate[] = [];

		this.plexServers?.forEach((x) => {
			serverUpdates.push({
				id: x.id,
				server: x,
				progress: this.inspectServerProgresses.find((y) => x.id === y.plexServerId) ?? null,
			});
		});

		return serverUpdates;
	}

	get getProgressText(): string {
		if (this.plexServers.length === 0) {
			return `Retrieving accessible servers for account ${this.account?.displayName ?? ''}.`;
		}
		if (this.getTotalPercentage === 100) {
			return `Completed inspection of ${this.plexServers.length} Plex servers!`;
		}
		return `Inspecting accessible servers, completed ${this.getCompletedCount} of ${this.plexServers.length}.`;
	}

	get getCompletedCount(): number {
		return this.inspectServerProgresses.filter((x) => x.completed).length;
	}

	get getTotalPercentage(): number {
		return clamp((this.getCompletedCount / this.plexServers.length) * 100, 0, 100);
	}

	mounted(): void {
		useSubscription(
			SignalrService.getAllInspectServerProgress().subscribe((data) => {
				this.inspectServerProgresses = data;
			}),
		);
	}
}
</script>
