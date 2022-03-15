<template>
	<div>
		<v-simple-table class="section-table">
			<tbody>
				<tr>
					<td style="width: 40%">{{ $t('components.server-dialog.tabs.server-config.download-speed-limit') }}:</td>
					<td>
						<download-limit-input :plex-server-id="plexServer.id" />
					</td>
				</tr>
			</tbody>
		</v-simple-table>
	</div>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import type { PlexServerDTO } from '@dto/mainApi';
@Component<ServerConfigTabContent>({})
export default class ServerConfigTabContent extends Vue {
	@Prop({ required: true, type: Object as () => PlexServerDTO })
	readonly plexServer!: PlexServerDTO;
}
</script>
