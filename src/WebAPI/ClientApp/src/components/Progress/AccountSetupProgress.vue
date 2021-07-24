<template>
	<v-card>
		<v-card-text>
			<!-- The total progress -->
			<progress-component
				:percentage="getTotalPercentage"
				:text="`Checking accessible servers ${getCompletedCount} of ${plexServers.length}`"
			/>
			<!--	Server Connection Details	-->
			<v-simple-table class="section-table">
				<tbody>
					<tr v-for="{ server, progress } in getServerUpdates" :key="server.id">
						<!--	Server name and status	-->
						<td style="width: 30%">
							<status :value="progress ? progress.connectionSuccessful && progress.completed : false" />
							{{ server.id }} - {{ server.name }}
						</td>
						<!--	Current Action	-->
						<td style="width: 35%">
							<template v-if="progress">
								<template v-if="!progress.completed">
									<span v-if="progress && progress.retryAttemptIndex > 0">
										Attempting to retry connection, {{ progress.retryAttemptIndex }} of {{ progress.retryAttemptCount }} attempt.
									</span>
								</template>
								<!--	Completed -->
								<template v-else>
									<span v-if="progress.connectionSuccessful"> <v-icon>mdi-check</v-icon> Server is connectable! </span>
									<span v-else> <v-icon>mdi-close</v-icon> Could not connect to server. </span>
								</template>
							</template>
						</td>
						<!--	Error message	-->
						<td style="width: 35%">
							<template v-if="progress">
								<span v-if="progress && progress.message">
									{{ progress.message }}
								</span>
							</template>
						</td>
					</tr>
				</tbody>
			</v-simple-table>
		</v-card-text>
		<v-card-actions>
			<p-btn :button-type="hideButtonType" />
			<p-btn :button-type="confirmButtonType" @click="refreshAccount(1)" />
		</v-card-actions>
	</v-card>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { AccountService, ProgressService, SignalrService } from '@service';
import { InspectServerProgress, PlexAccountRefreshProgress, PlexServerDTO } from '@dto/mainApi';
import ButtonType from '@enums/buttonType';
import { refreshAccount } from '@api/accountApi';
import { switchMap } from 'rxjs';
import { inspectPlexServers } from '@api/plexServerApi';
import { tap } from 'rxjs/operators';
import _ from 'lodash';

declare interface ServerUpdate {
	id: number;
	server: PlexServerDTO;
	progress: InspectServerProgress | null;
}

@Component
export default class AccountSetupProgress extends Vue {
	accountSetupProgress: PlexAccountRefreshProgress | null = null;
	inspectServerProgresses: InspectServerProgress[] = [];

	hideButtonType: ButtonType = ButtonType.Hide;
	confirmButtonType: ButtonType = ButtonType.Confirm;
	plexServers: PlexServerDTO[] = [];

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

	get getCompletedCount(): number {
		return this.inspectServerProgresses.filter((x) => x.completed).length;
	}

	get getTotalPercentage(): number {
		return _.clamp((this.getCompletedCount / this.plexServers.length) * 100, 0, 100);
	}

	refreshAccount(accountId: number = 0): void {
		this.$subscribeTo(ProgressService.getPlexAccountRefreshProgress(), (progress) => {
			if (progress) {
				this.accountSetupProgress = progress;
			}
		});

		this.$subscribeTo(
			refreshAccount(accountId).pipe(
				// Get account with accessible plexServers
				switchMap(() => AccountService.fetchAccount(accountId)),
				// Save plexServers
				tap((account) => {
					if (account) {
						this.plexServers = account.plexServers;
					}
				}),
				// Check status and connectivity of all plexServers
				switchMap(() =>
					inspectPlexServers(
						accountId,
						this.plexServers.map((x) => x.id),
					),
				),
			),
			() => {},
		);
	}

	created(): void {
		this.$subscribeTo(SignalrService.getAllInspectServerProgress(), (data) => {
			this.inspectServerProgresses = data;
		});
	}
}
</script>
