<template>
	<v-btn class="vertical-button" text tile :height="height" :disabled="disabled" :width="width" @click="$emit('click', $event)">
		<div class="vertical-button-content">
			<v-icon x-large>{{ icon }}</v-icon>
			<span class="vertical-button-label">{{ label }}</span>
		</div>
	</v-btn>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';

@Component
export default class VerticalButton extends Vue {
	@Prop({ required: true, type: String })
	readonly icon!: string;

	@Prop({ required: true, type: String })
	readonly label!: string;

	@Prop({ required: true, type: Number })
	readonly height!: number;

	@Prop({ required: false, type: Number })
	readonly width!: number;

	@Prop({ required: false, type: Boolean, default: false })
	readonly disabled!: boolean;
}
</script>
