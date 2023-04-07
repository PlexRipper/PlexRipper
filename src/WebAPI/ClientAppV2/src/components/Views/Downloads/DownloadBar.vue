<template>
	<!-- Download Toolbar -->
	<v-row no-gutters>
		<v-col>
			<v-toolbar outlined :height="64">
				<!--Prioritize buttons-->
				<!--		Re-enable once ordering had been implemented		-->
				<template v-if="false">
					<v-btn-toggle borderless group tile :max="0">
						<v-btn>
							<v-icon large>mdi-arrow-collapse-up</v-icon>
						</v-btn>

						<v-btn>
							<v-icon large>mdi-arrow-up</v-icon>
						</v-btn>

						<v-btn>
							<v-icon large>mdi-arrow-down</v-icon>
						</v-btn>

						<v-btn>
							<v-icon large>mdi-arrow-collapse-down</v-icon>
						</v-btn>
					</v-btn-toggle>

					<v-spacer />
				</template>

				<!--Command buttons-->
				<v-btn
					v-for="(button, i) in buttons"
					:key="i"
					depressed
					tile
					:disabled="button.disableOnNoSelected && !hasSelected"
					class="no-background"
					@click="$emit(button.value)"
				>
					<v-icon large left>{{ button.icon }}</v-icon>
					<span class="hidden-sm-and-down">{{ button.name }}</span>
				</v-btn>
			</v-toolbar>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';

interface buttons {
	name: string;
	value: string;
	icon: string;
	disableOnNoSelected: boolean;
}

@Component
export default class DownloadBar extends Vue {
	@Prop({ required: true, type: Boolean })
	readonly hasSelected!: boolean;

	get buttons(): buttons[] {
		return [
			{
				name: 'Clear Completed',
				value: 'clear',
				icon: 'mdi-notification-clear-all',
				disableOnNoSelected: true,
			},
			// {
			// 	name: 'Start',
			// 	value: 'start',
			// 	icon: 'mdi-play',
			// 	disableOnNoSelected: true,
			// },
			// {
			// 	name: 'Pause',
			// 	value: 'pause',
			// 	icon: 'mdi-pause',
			// 	disableOnNoSelected: true,
			// },
			// {
			// 	name: 'Stop',
			// 	value: 'stop',
			// 	icon: 'mdi-stop',
			// 	disableOnNoSelected: true,
			// },
			// {
			// 	name: 'Restart',
			// 	value: 'restart',
			// 	icon: 'mdi-restart',
			// 	disableOnNoSelected: true,
			// },
			{
				name: 'Delete',
				value: 'delete',
				icon: 'mdi-delete',
				disableOnNoSelected: true,
			},
		];
	}
}
</script>
