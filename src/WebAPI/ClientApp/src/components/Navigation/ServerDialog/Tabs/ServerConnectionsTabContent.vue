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
				<tr v-for="(connection, index) in serverConnections" :key="index">
					<td>
						<v-radio color="red" :value="connection.id" :label="connection.url" />
					</td>
					<td style="width: 25%">
						<status :value="connection.plexServerStatus[0]?.isSuccessful ?? false" />
					</td>
					<td>
						<CheckConnectionButton :loading="isLoading(connection.id)" @click="checkPlexConnection(connection.id)" />
					</td>
				</tr>
			</tbody>
		</v-simple-table>
	</v-radio-group>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import type { PlexServerConnectionDTO, PlexServerDTO } from '@dto/mainApi';
import { ServerConnectionService } from '@service';

@Component<ServerConnectionsTabContent>({})
export default class ServerConnectionsTabContent extends Vue {
	@Prop({ required: true, type: Object as () => PlexServerDTO })
	readonly plexServer!: PlexServerDTO;

	@Prop({ required: true, type: Boolean })
	readonly isVisible!: boolean;

	serverConnections: PlexServerConnectionDTO[] = [];
	value: any = '';
	loading: number[] = [];

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

	@Watch('isVisible')
	onIsVisible(isVisible): void {
		if (isVisible) {
			this.setup();
		}
	}

	setup() {
		useSubscription(
			ServerConnectionService.getServerConnectionsByServerId(this.plexServer.id).subscribe((connections) => {
				this.serverConnections = connections;
			}),
		);
	}

	mounted() {
		this.setup();
	}
}
</script>
