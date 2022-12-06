<template>
	<v-simple-table class="section-table">
		<tbody>
			<tr>
				<td>{{ $t('components.server-dialog.tabs.server-commands.inspect-server') }}</td>
				<td>
					<BaseButton :disabled="syncLoading" :loading="inspectLoading" text-id="inspect-server" />
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
import { Component, Prop, Vue } from 'vue-property-decorator';
import { syncPlexServer } from '@api/plexServerApi';
import type { PlexServerDTO } from '@dto/mainApi';

@Component<ServerCommandsTabContent>({})
export default class ServerCommandsTabContent extends Vue {
	@Prop({ required: true, type: Object as () => PlexServerDTO })
	readonly plexServer!: PlexServerDTO;

	@Prop({ required: true, type: Boolean })
	readonly isVisible!: boolean;

	syncLoading: boolean = false;
	inspectLoading: boolean = false;

	syncServerLibraries(): void {
		this.syncLoading = true;
		syncPlexServer(this.plexServer.id, true).subscribe(() => {
			this.syncLoading = false;
		});
	}
}
</script>
