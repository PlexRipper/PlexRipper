<template>
	<v-card class="account-setup-progress">
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
			<v-simple-table v-if="plexServers.length > 0" class="section-table">
				<tbody>
					<template v-for="server in plexServers">
						<InspectServerProgressDisplay
							:key="server.id"
							:plex-server-name="server.name"
							:plex-server-id="server.id"
							@completed="setComplete($event)"
						/>
					</template>
				</tbody>
			</v-simple-table>
			<!-- No Server Warning	-->
			<v-row v-else justify="center">
				<v-col cols="auto">
					<h2>
						{{ noServerWarning }}
					</h2>
				</v-col>
			</v-row>
		</v-card-text>
		<v-card-actions>
			<v-row justify="end">
				<v-col cols="auto">
					<HideButton @click="$emit('hide')" />
				</v-col>
			</v-row>
		</v-card-actions>
	</v-card>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import { clamp } from 'lodash-es';
import Log from 'consola';
import { ServerService } from '@service';
import type { PlexAccountDTO, PlexServerDTO } from '@dto/mainApi';

@Component
export default class AccountSetupProgress extends Vue {
	@Prop({ required: true, type: Object as () => PlexAccountDTO })
	readonly account!: PlexAccountDTO;

	plexServers: PlexServerDTO[] = [];

	completedPlexServers: Number[] = [];

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
		return this.completedPlexServers.length;
	}

	get getTotalPercentage(): number {
		return clamp((this.getCompletedCount / this.plexServers.length) * 100, 0, 100);
	}

	get noServerWarning(): string {
		return this.$t('components.account-setup-progress.no-server-warning', {
			displayName: this.account ? this.account.displayName : this.$t('general.commands.unknown'),
		}).toString();
	}

	setComplete(plexServerId: number) {
		this.completedPlexServers.push(plexServerId);
	}

	mounted(): void {
		Log.info('Mounted was fired!');
		useSubscription(
			ServerService.getServersByPlexAccountId(this.account.id).subscribe((servers) => {
				this.plexServers = servers;
			}),
		);
	}
}
</script>
