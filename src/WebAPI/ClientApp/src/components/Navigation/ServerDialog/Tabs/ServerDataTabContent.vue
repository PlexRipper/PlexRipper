<template>
	<div>
		<!--	Server Data Tab Content	-->
		<v-simple-table class="section-table">
			<tbody>
				<!-- Machine Identifier -->
				<tr>
					<td style="width: 25%">{{ $t('components.server-dialog.tabs.server-data.machine-id') }}:</td>
					<td>{{ plexServer.machineIdentifier }}</td>
				</tr>
				<!-- Device -->
				<tr>
					<td>{{ $t('components.server-dialog.tabs.server-data.device') }}:</td>
					<td>{{ plexServer.device }}</td>
				</tr>
				<!-- Platform and platform version -->
				<tr>
					<td>{{ $t('components.server-dialog.tabs.server-data.platform') }}:</td>
					<td>{{ plexServer.platform }} ({{ plexServer.platformVersion }})</td>
				</tr>
				<!-- Product and version -->
				<tr>
					<td>{{ $t('components.server-dialog.tabs.server-data.plex-version') }}:</td>
					<td>{{ plexServer.product }} ({{ plexServer.productVersion }})</td>
				</tr>
				<!-- Created On -->
				<tr>
					<td>{{ $t('components.server-dialog.tabs.server-data.created-on') }}:</td>
					<td>
						<date-time short-date :text="plexServer.createdAt" />
					</td>
				</tr>
				<tr>
					<td>{{ $t('components.server-dialog.tabs.server-data.last-updated-on') }}:</td>
					<td>
						<date-time short-date :text="plexServer.lastSeenAt" />
					</td>
				</tr>
				<tr>
					<td>{{ $t('components.server-dialog.tabs.server-data.current-status') }}:</td>
					<td>
						<status pulse :value="hasSuccessServerStatus" />
					</td>
				</tr>
				<!-- Server Status -->
				<tr v-if="serverConnections.length">
					<td>{{ $t('components.server-dialog.tabs.server-data.last-checked-on') }}:</td>
					<td>
						<date-time short-date :text="getLastStatusCheck" />
					</td>
				</tr>
			</tbody>
		</v-simple-table>
		<!--	Check Server Action	-->
		<v-card-actions>
			<v-spacer></v-spacer>
			<BaseButton text-id="check-server-status" :loading="checkServerStatusLoading" @click="checkServer" />
		</v-card-actions>
	</div>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import type { PlexServerDTO } from '@dto/mainApi';
import { PlexServerConnectionDTO } from '@dto/mainApi';
import { ServerConnectionService, ServerService } from '@service';

@Component
export default class ServerDataTabContent extends Vue {
	@Prop({ required: true, type: Object as () => PlexServerDTO })
	readonly plexServer!: PlexServerDTO;

	@Prop({ required: true, type: Boolean })
	readonly isVisible!: boolean;

	checkServerStatusLoading: boolean = false;
	hasSuccessServerStatus: boolean = false;
	serverConnections: PlexServerConnectionDTO[] = [];

	@Watch('isVisible')
	onIsVisible(isVisible): void {
		if (isVisible) {
			this.setup();
		}
	}

	get getLastStatusCheck(): string {
		const y = this.serverConnections
			.map((x) => x.latestConnectionStatus)
			.sort((a, b) => Date.parse(b.lastChecked) - Date.parse(a.lastChecked));
		return y[0].lastChecked;
	}

	checkServer(): void {
		this.checkServerStatusLoading = true;
		useSubscription(
			ServerService.checkServer(this.plexServer.id).subscribe(() => {
				this.checkServerStatusLoading = false;
			}),
		);
	}

	setup() {
		useSubscription(
			ServerConnectionService.getServerConnectionsByServerId(this.plexServer.id).subscribe((connections) => {
				this.serverConnections = connections;
			}),
		);
		useSubscription(
			ServerService.getServerStatus(this.plexServer.id).subscribe((value) => {
				this.hasSuccessServerStatus = value;
			}),
		);
	}

	mounted() {
		this.setup();
	}
}
</script>
