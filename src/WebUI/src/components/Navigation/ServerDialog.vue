<template>
	<v-dialog :value="show" max-width="800" @click:outside="close">
		<v-card v-if="plexServer">
			<v-card-title class="headline">{{ plexServer.name }} configuration</v-card-title>

			<v-card-text>
				<v-container>
					<v-row>
						<v-col>
							<v-simple-table class="section-table">
								<tbody>
									<tr>
										<td style="width: 25%">Server URL:</td>
										<td>{{ plexServer.serverUrl }}</td>
									</tr>
									<tr>
										<td>Machine Identifier:</td>
										<td>{{ plexServer.machineIdentifier }}</td>
									</tr>
									<tr>
										<td>Plex Version:</td>
										<td>{{ plexServer.version }}</td>
									</tr>
									<tr>
										<td>Created On:</td>
										<td><date-time short-date :text="plexServer.createdAt" /></td>
									</tr>
									<tr>
										<td>Last updated on:</td>
										<td><date-time short-date :text="plexServer.updatedAt" /></td>
									</tr>
									<tr v-if="serverStatus">
										<td>Current status:</td>
										<td>
											<status pulse :value="serverStatus.isSuccessful" />
											{{ serverStatus.statusCode }} -
											{{ serverStatus.statusMessage }}
										</td>
									</tr>
									<tr v-if="serverStatus">
										<td>Last checked on:</td>
										<td><date-time short-date :text="serverStatus.lastChecked" /></td>
									</tr>
								</tbody>
							</v-simple-table>
						</v-col>
					</v-row>
				</v-container>
			</v-card-text>

			<!--	Close action	-->
			<v-card-actions>
				<v-spacer></v-spacer>
				<p-btn text-id="check-server-status" @click="checkServer" />
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import { PlexServerDTO, PlexServerStatusDTO } from '@dto/mainApi';
import ServerService from '@service/serverService';
import { map } from 'rxjs/operators';
import Log from 'consola';

@Component
export default class ServerDialog extends Vue {
	@Prop({ required: true, type: Number, default: 0 })
	readonly serverId!: number;

	show: boolean = false;

	plexServer: PlexServerDTO | null = null;
	@Watch('serverId')
	onServerIdChanged(id: number) {
		this.show = id > 0;
		if (this.show) {
			this.getServerData();
		}
	}

	get serverStatus(): PlexServerStatusDTO | null {
		Log.warn(this.plexServer?.status);
		return this.plexServer?.status ?? null;
	}

	getServerData(): void {
		ServerService.getServers()
			.pipe(map((server) => server.find((x) => x.id === this.serverId)))
			.subscribe((server) => {
				if (server) {
					this.plexServer = server;
				}
			});
		ServerService.getServer(this.serverId);
	}

	checkServer(): void {
		ServerService.checkServer(this.serverId);
	}

	close(): void {
		this.$emit('close');
	}
}
</script>
