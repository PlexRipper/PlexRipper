<template>
	<v-dialog :value="show" max-width="600" @click:outside="close">
		<v-card v-if="plexServer">
			<v-card-title class="headline">{{ plexServer.name }} configuration</v-card-title>

			<v-card-text>
				<v-container>
					<v-row>
						<v-col>
							<v-simple-table>
								<template v-slot:default>
									<tbody>
										<tr>
											<td>Server URL:</td>
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
											<td><date-time :text="plexServer.createdAt" /></td>
										</tr>
										<tr>
											<td>Last updated on:</td>
											<td><date-time :text="plexServer.updatedAt" /></td>
										</tr>
									</tbody>
								</template>
							</v-simple-table>
						</v-col>
					</v-row>
				</v-container>
			</v-card-text>

			<!--	Close action	-->
			<v-card-actions>
				<v-spacer></v-spacer>
				<p-btn text-id="check-server-status" />
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import { PlexServerDTO } from '@dto/mainApi';
import { getPlexServer } from '@api/plexServerApi';
@Component
export default class ServerDialog extends Vue {
	@Prop({ required: true, type: Number, default: 0 })
	readonly serverId!: number;

	show: boolean = false;

	plexServer: PlexServerDTO | null = null;
	@Watch('serverId')
	onServerIdChanged(id: number) {
		this.show = id > 0;
		this.getServerData();
	}

	getServerData(): void {
		if (this.serverId > 0) {
			getPlexServer(this.serverId).subscribe((data) => {
				this.plexServer = data;
			});
		}
	}

	close(): void {
		this.$emit('close');
	}
}
</script>
