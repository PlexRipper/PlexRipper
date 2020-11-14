<template>
	<v-btn text tile :height="height" @click="buttonClick">
		<div class="hello-text">
			<v-icon x-large>{{ icon }}</v-icon>
			<v-subheader class="vertical-button-label">{{ label }}</v-subheader>
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

	buttonClick(e: MouseEvent): void {
		this.$emit('click', e);
	}
}
</script>

<style lang="scss">
.hello-text {
	display: flex;
	flex-direction: column;
	align-items: center;
}
.vertical-button-label {
	height: 24px;
}
</style>
