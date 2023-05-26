<template>
	<v-row class="no-wrap">
		<v-col :cols="8">
			<v-slider
				:value="value"
				:min="0"
				:step="500"
				:max="10000"
				style="margin-top: 8px"
				@input="sliderValue = $event"
				@change="updateDownloadLimit"
				@mousedown="mouseEvent = true"
				@mouseup="mouseEvent = false"
			/>
		</v-col>
		<v-col>
			<p-text-field
				:value="value"
				full-width
				hide-details
				single-line
				type="number"
				suffix="kB/s"
				hide-spin-buttons
				@click.prevent
				@change="updateDownloadLimit"
			/>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';

@Component<DownloadLimitInput>({})
export default class DownloadLimitInput extends Vue {
	@Prop({ required: true, type: Number })
	readonly plexServerId!: number;

	@Prop({ required: true, type: Number })
	readonly downloadSpeedLimit!: number;

	mouseEvent: boolean = false;
	sliderValue: number = 0;

	get value(): number {
		if (this.mouseEvent) {
			return this.sliderValue;
		}
		return this.downloadSpeedLimit;
	}

	updateDownloadLimit(value) {
		value = Number(value);
		if (value < 0) {
			value = 0;
		}
		this.$emit('change', value);
	}
}
</script>
<!--suppress CssUnusedSymbol -->
<style>
/* This is to make the v-slider less high and help vertical center the label in front. */
.v-messages {
	display: none !important;
}
</style>
