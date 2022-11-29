<template>
	<v-simple-table class="section-table">
		<tbody>
			<tr>
				<td>{{ $t('components.server-dialog.tabs.server-commands.inspect-server') }}</td>
				<td>
					<BaseButton
						:disabled="syncLoading"
						:loading="inspectLoading"
						text-id="inspect-server"
						@click="inspectServer"
					/>
				</td>
			</tr>
			<tr v-if="displayInspectProgress">
				<td colspan="2">
					<v-simple-table class="section-table">
						<InspectServerProgressDisplay :plex-server-name="plexServer.name" :plex-server-id="plexServer.id" />
					</v-simple-table>
				</td>
			</tr>
			<tr>
				<td>{{ $t('components.server-dialog.tabs.server-commands.re-sync-server') }}</td>
				<td>
					<BaseButton
						:disabled="inspectLoading"
						:loading="syncLoading"
						text-id="sync-server-libraries"
						@click="syncServerLibraries"
					/>
				</td>
			</tr>
		</tbody>
	</v-simple-table>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import { inspectPlexServer, syncPlexServer } from '@api/plexServerApi';
import type { PlexServerDTO } from '@dto/mainApi';

@Component<ServerCommandsTabContent>({})
export default class ServerCommandsTabContent extends Vue {
	@Prop({ required: true, type: Object as () => PlexServerDTO })
	readonly plexServer!: PlexServerDTO;

	@Prop({ required: true, type: Boolean })
	readonly isVisible!: boolean;

	syncLoading: boolean = false;
	inspectLoading: boolean = false;
	displayInspectProgress: boolean = false;

	syncServerLibraries(): void {
		this.syncLoading = true;
		syncPlexServer(this.plexServer.id, true).subscribe(() => {
			this.syncLoading = false;
		});
	}

	inspectServer(): void {
		this.displayInspectProgress = true;
		this.inspectLoading = true;
		inspectPlexServer(this.plexServer.id).subscribe(() => {
			this.inspectLoading = false;
		});
	}

	@Watch('isVisible')
	onIsVisible(isVisible): void {
		if (!isVisible) {
			this.displayInspectProgress = false;
		}
	}
}
</script>
