<template>
	<v-radio-group v-model="value" dense class="mt-0 pt-0">
		<v-simple-table class="section-table">
			<thead>
				<tr>
					<td colspan="3">
						<h3>{{ $t('components.server-dialog.tabs.server-connections.section-header') }}</h3>
					</td>
				</tr>
			</thead>
			<tbody>
				<template v-for="(connection, index) in serverConnections">
					<tr :key="`connection-${index}`">
						<td>
							<v-radio color="red" :value="connection.id">
								<template #label>
									<!-- Connection Icon Local or Public -->
									<v-tooltip top>
										<template #activator="{ on, attrs }">
											<v-icon v-bind="attrs" v-on="on">
												{{ getConnectionIcon(connection.local) }}
											</v-icon>
										</template>
										<span>{{
											connection.local
												? $t('general.tooltip.local-connection')
												: $t('general.tooltip.public-connection')
										}}</span>
									</v-tooltip>
									<!-- Connection Url -->
									<span class="ml-2">{{ connection.url }}</span>
								</template>
							</v-radio>
						</td>
						<td style="width: 25%">
							<!-- Connection Status -->
							<status :value="connection.plexServerStatus[0]?.isSuccessful ?? false" />
						</td>
						<td>
							<CheckConnectionButton
								:loading="isLoading(connection.id)"
								@click="checkPlexConnection(connection.id)"
							/>
						</td>
					</tr>
					<tr v-if="getProgress(connection.id)" :key="`progress-${index}`">
						<td colspan="3">
							<CheckServerStatusProgressDisplay :plex-server="plexServer" :progress="getProgress(connection.id)" />
						</td>
					</tr>
				</template>
			</tbody>
		</v-simple-table>
	</v-radio-group>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import type { PlexServerConnectionDTO, PlexServerDTO } from '@dto/mainApi';
import { ServerConnectionCheckStatusProgressDTO } from '@dto/mainApi';
import { ServerConnectionService, SignalrService } from '@service';

@Component<ServerConnectionsTabContent>({})
export default class ServerConnectionsTabContent extends Vue {
	@Prop({ required: true, type: Object as () => PlexServerDTO })
	readonly plexServer!: PlexServerDTO;

	@Prop({ required: true, type: Boolean })
	readonly isVisible!: boolean;

	serverConnections: PlexServerConnectionDTO[] = [];
	value: any = '';
	loading: number[] = [];
	progress: ServerConnectionCheckStatusProgressDTO[] = [];

	getProgress(plexServerConnectionId: number): ServerConnectionCheckStatusProgressDTO | null {
		return this.progress.find((x) => x.plexServerConnectionId === plexServerConnectionId) ?? null;
	}

	getConnectionIcon(local: boolean): string {
		return local ? 'mdi-lan-connect' : 'mdi-earth';
	}

	@Watch('isVisible')
	onIsVisible(isVisible): void {
		if (isVisible) {
			this.setup();
		} else {
			this.progress = [];
		}
	}

	isLoading(plexServerConnectionId: number): boolean {
		return this.loading.includes(plexServerConnectionId);
	}

	checkPlexConnection(plexServerConnectionId: number) {
		this.loading.push(plexServerConnectionId);
		useSubscription(
			ServerConnectionService.checkServerConnection(plexServerConnectionId).subscribe(() => {
				this.loading = this.loading.filter((x) => x !== plexServerConnectionId);
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
			SignalrService.getServerConnectionProgressByPlexServerId(this.plexServer.id).subscribe((progress) => {
				this.progress = progress;
			}),
		);
	}

	mounted() {
		this.setup();
	}
}
</script>
