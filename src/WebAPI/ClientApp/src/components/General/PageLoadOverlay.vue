<template>
	<div>
		<v-overlay v-if="overlayState" :value="overlayState" :opacity="0">
			<div class="logo-square">
				<logo :size="256" @click="overlayState = false" />
			</div>
		</v-overlay>
		<slot v-else></slot>
	</div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { merge, timer } from 'rxjs';
import globalService from '~/service/globalService';

@Component<PageLoadOverlay>({
	components: {},
})
export default class PageLoadOverlay extends Vue {
	overlayState: boolean = true;

	mounted() {
		this.$subscribeTo(merge([timer(5000), globalService.getPageSetupReady()]), () => {
			this.overlayState = false;
		});
	}
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
