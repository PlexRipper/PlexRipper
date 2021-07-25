<template>
	<v-card>
		<v-card-text>
			<!-- The total progress -->
			<progress-component
				:percentage="getTotalPercentage"
				:completed="getTotalPercentage === 100 || noServers"
				:text="getProgressText"
				circular-mode
				:indeterminate="plexServers.length === 0 && !noServers"
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
										Attempting to retry connection, {{ progress.retryAttemptIndex }} of {{ progress.retryAttemptCount }} attempt.
									</span>
								</template>
								<!--	Completed -->
								<template v-else>
									<span v-if="progress.connectionSuccessful"> Server is connectable! </span>
									<span v-else> Could not connect to server. </span>
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
					<h2>Account {{ account ? account.displayName : 'unknown' }} has no servers which it can access.</h2>
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
import _ from 'lodash';
import { Component, Prop, Vue } from 'vue-property-decorator';
import { of, switchMap } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { AccountService, SignalrService } from '@service';
import type { InspectServerProgress, PlexAccountDTO, PlexServerDTO } from '@dto/mainApi';
import ButtonType from '@enums/buttonType';
import { refreshAccount } from '@api/accountApi';
import { inspectPlexServers } from '@api/plexServerApi';
import Log from 'consola';

declare interface ServerUpdate {
	id: number;
	server: PlexServerDTO;
	progress: InspectServerProgress | null;
}

@Component
export default class AccountSetupProgress extends Vue {
	@Prop({ required: true, type: Number })
	readonly accountId!: number;

	account: PlexAccountDTO | null = null;
	inspectServerProgresses: InspectServerProgress[] = [];

	hideButtonType: ButtonType = ButtonType.Hide;
	confirmButtonType: ButtonType = ButtonType.Confirm;
	plexServers: PlexServerDTO[] = [];

	/**
	 * Triggered when the API responds with no Plex servers.
	 * @type {boolean}
	 */
	noServers: boolean = false;

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
		return _.clamp((this.getCompletedCount / this.plexServers.length) * 100, 0, 100);
	}

	refreshAccount(accountId: number = 0): void {
		this.$subscribeTo(
			refreshAccount(accountId).pipe(
				// Get account with accessible plexServers
				switchMap(() => AccountService.fetchAccount(accountId)),
				// Save plexServers
				tap((account) => {
					if (account) {
						this.plexServers = account.plexServers;
						this.noServers = this.plexServers.length === 0;
					}
				}),
				// Check status and connectivity of all plexServers
				switchMap(() => {
					if (!this.noServers) {
						return inspectPlexServers(
							accountId,
							this.plexServers.map((x) => x.id),
						);
					}
					return of(null);
				}),
			),
			() => {
				this.$emit('complete');
			},
		);
	}

	mounted(): void {
		this.$subscribeTo(SignalrService.getAllInspectServerProgress(), (data) => {
			this.inspectServerProgresses = data;
		});

		this.$subscribeTo(
			this.$watchAsObservable('accountId').pipe(
				map((x) => x.newValue),
				tap((x) => Log.debug('accountId', x)),
				switchMap((x) => AccountService.getAccount(x)),
			),
			(account) => {
				this.account = account;
			},
		);
	}
}
</script>
