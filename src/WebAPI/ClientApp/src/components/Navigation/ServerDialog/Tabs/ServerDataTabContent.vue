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
				<tr v-if="serverStatus">
					<td>{{ $t('components.server-dialog.tabs.server-data.current-status') }}:</td>
					<td>
						<status pulse :value="serverStatus.isSuccessful" />
						{{ serverStatus.statusCode }} -
						{{ serverStatus.statusMessage }}
					</td>
				</tr>
				<!--	Server Status	-->
				<tr v-if="serverStatus">
					<td>{{ $t('components.server-dialog.tabs.server-data.last-checked-on') }}:</td>
					<td>
						<date-time short-date :text="serverStatus.lastChecked" />
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
import { Component, Prop, Vue } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import type { PlexServerDTO, PlexServerStatusDTO } from '@dto/mainApi';
import { ServerService } from '@service';

@Component<ServerDataTabContent>({})
export default class ServerDataTabContent extends Vue {
	@Prop({ required: true, type: Object as () => PlexServerDTO })
	readonly plexServer!: PlexServerDTO;

	@Prop({ required: true, type: Object as () => PlexServerStatusDTO })
	readonly serverStatus!: PlexServerStatusDTO;

	checkServerStatusLoading: boolean = false;

	checkServer(): void {
		this.checkServerStatusLoading = true;
		useSubscription(
			ServerService.checkServer(this.plexServer.id).subscribe(() => {
				this.checkServerStatusLoading = false;
			}),
		);
	}
}
</script>
