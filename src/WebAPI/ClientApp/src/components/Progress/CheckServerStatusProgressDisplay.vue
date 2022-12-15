<template>
	<tr>
		<!--	Status icon	-->
		<td style="width: 10%">
			<template v-if="progress">
				<v-progress-circular v-if="!progress.completed" indeterminate color="red" />
				<v-icon v-else-if="progress.connectionSuccessful">mdi-check</v-icon>
				<v-icon v-else>mdi-close</v-icon>
			</template>
			<template v-else>
				<v-progress-circular indeterminate color="red" />
			</template>
		</td>
		<!--	Current Action	-->
		<td style="width: 45%">
			<ConnectionProgressText :progress="progress" />
		</td>
		<!--	Error message	-->
		<td style="width: 45%">
			<template v-if="progress && !progress.completed">
				<span v-if="progress && progress.message">
					{{ progress.message }}
				</span>
			</template>
		</td>
	</tr>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import type { PlexServerDTO, ServerConnectionCheckStatusProgressDTO } from '@dto/mainApi';

@Component
export default class CheckServerStatusProgressDisplay extends Vue {
	@Prop({ required: true, type: Object as () => PlexServerDTO })
	readonly plexServer!: PlexServerDTO;

	@Prop({ required: false, type: Object as () => ServerConnectionCheckStatusProgressDTO })
	readonly progress!: ServerConnectionCheckStatusProgressDTO;
}
</script>
