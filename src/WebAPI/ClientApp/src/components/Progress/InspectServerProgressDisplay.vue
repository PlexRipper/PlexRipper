<template>
	<tr>
		<!--	Server name and status	-->
		<td style="width: 30%">
			<status :value="progress ? progress.connectionSuccessful && progress.completed : false" />
			{{ plexServerName }}
		</td>
		<!--	Status icon	-->
		<td style="width: 10%">
			<!--	Plex Connection Status Progress Icon -->
			<boolean-progress :loading="!progress || !progress.completed" :success="progress.connectionSuccessful" />
		</td>
		<!--	Current Action	-->
		<td style="width: 30%">
			<ConnectionProgressText :progress="progress" />
		</td>
		<!--	Error message	-->
		<td style="width: 30%">
			<template v-if="progress">
				<span v-if="progress && progress.message">
					{{ progress.message }}
				</span>
			</template>
		</td>
	</tr>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import { SignalrService } from '@service';
import { InspectServerProgressDTO } from '@dto/mainApi';
import ConnectionProgressText from '@components/Progress/ConnectionProgressText.vue';

@Component({
	components: { ConnectionProgressText },
})
export default class InspectServerProgressDisplay extends Vue {
	@Prop({ required: true, type: Number })
	readonly plexServerId!: number;

	@Prop({ required: true, type: String })
	readonly plexServerName!: string;

	progress: InspectServerProgressDTO | null = null;

	mounted(): void {
		useSubscription(
			SignalrService.getInspectServerProgress(this.plexServerId).subscribe((data) => {
				this.progress = data;
				if (data?.completed) {
					this.$emit('completed', this.plexServerId);
				}
			}),
		);
	}
}
</script>
