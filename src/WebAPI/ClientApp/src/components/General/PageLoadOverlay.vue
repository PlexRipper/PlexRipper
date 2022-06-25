<template>
	<v-overlay :value="value" :opacity="0">
		<div class="logo-square">
			<logo :size="256" />
		</div>
	</v-overlay>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';

@Component<PageLoadOverlay>({})
export default class PageLoadOverlay extends Vue {
	@Prop({ required: true, type: Boolean })
	readonly value!: boolean;
}
</script>
<style>
.logo-square {
	width: 256px;
	height: 256px;
	animation: rotateAnimation 3s cubic-bezier(0.25, 0.46, 0.45, 0.94) infinite;
}

@keyframes rotateAnimation {
	from {
		transform: rotateY(0deg);
	}
	to {
		transform: rotateY(359deg);
	}
}
</style>
