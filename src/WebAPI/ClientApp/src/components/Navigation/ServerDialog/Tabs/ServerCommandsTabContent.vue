<template>
	<v-simple-table class="section-table">
		<tbody>
			<tr>
				<td>{{ $t('components.server-dialog.tabs.server-commands.re-sync-server') }}</td>
				<td>
					<p-btn text-id="sync-server-libraries" @click="syncServerLibraries" />
				</td>
			</tr>
		</tbody>
	</v-simple-table>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import { syncPlexServer } from '@api/plexServerApi';
import type { PlexServerDTO } from '@dto/mainApi';

@Component<ServerCommandsTabContent>({
	components: {},
})
export default class ServerCommandsTabContent extends Vue {
	@Prop({ required: true, type: Object as () => PlexServerDTO })
	readonly plexServer!: PlexServerDTO;

	syncServerLibraries(): void {
		syncPlexServer(this.plexServer.id, true).subscribe();
	}
}
</script>
