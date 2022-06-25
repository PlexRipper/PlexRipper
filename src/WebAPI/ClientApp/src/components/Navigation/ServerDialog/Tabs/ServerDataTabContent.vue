<template>
	<div>
		<!--	Server Data Tab Content	-->
		<v-simple-table class="section-table">
			<tbody>
				<tr>
					<td style="width: 25%">{{ $t('components.server-dialog.tabs.server-data.server-url') }}:</td>
					<td>{{ plexServer.serverUrl }}</td>
				</tr>
				<tr>
					<td>{{ $t('components.server-dialog.tabs.server-data.machine-id') }}:</td>
					<td>{{ plexServer.machineIdentifier }}</td>
				</tr>
				<tr>
					<td>{{ $t('components.server-dialog.tabs.server-data.plex-version') }}:</td>
					<td>{{ plexServer.version }}</td>
				</tr>
				<tr>
					<td>{{ $t('components.server-dialog.tabs.server-data.created-on') }}:</td>
					<td><date-time short-date :text="plexServer.createdAt" /></td>
				</tr>
				<tr>
					<td>{{ $t('components.server-dialog.tabs.server-data.last-updated-on') }}:</td>
					<td><date-time short-date :text="plexServer.updatedAt" /></td>
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
					<td><date-time short-date :text="serverStatus.lastChecked" /></td>
				</tr>
			</tbody>
		</v-simple-table>
		<!--	Check Server Action	-->
		<v-card-actions>
			<v-spacer></v-spacer>
			<p-btn text-id="check-server-status" @click="checkServer" />
		</v-card-actions>
	</div>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import type { PlexServerDTO, PlexServerStatusDTO } from '@dto/mainApi';
import { ServerService } from '@service';

@Component<ServerDataTabContent>({
	components: {},
})
export default class ServerDataTabContent extends Vue {
	@Prop({ required: true, type: Object as () => PlexServerDTO })
	readonly plexServer!: PlexServerDTO;

	@Prop({ required: true, type: Object as () => PlexServerStatusDTO })
	readonly serverStatus!: PlexServerStatusDTO;

	checkServer(): void {
		ServerService.checkServer(this.plexServer.id);
	}
}
</script>
