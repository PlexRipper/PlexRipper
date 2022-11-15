<template>
	<div :class="backgroundEffect">
		<div :class="backgroundOverlay">
			<slot />
		</div>
	</div>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Prop, Vue } from 'vue-property-decorator';
import * as THREE from 'three';
import WAVES from 'vanta/dist/vanta.waves.min';
import WebGL from '@class/WebGL';

@Component
export default class Background extends Vue {
	vantaEffect: any;

	@Prop({ type: Boolean, default: false })
	readonly hideBackground!: Boolean;

	get isDark(): boolean {
		return this.$vuetify.theme.dark;
	}

	get backgroundEffect(): any {
		return {
			'background-effect': true,
			'still-background-effect': !this.vantaEffect,
		};
	}

	get backgroundOverlay(): any {
		if (this.hideBackground) {
			return {
				'background-overlay': true,
				'no-background': true,
			};
		}
		return {
			'background-overlay': true,
			'dark-background': this.isDark,
			'light-background': !this.isDark,
		};
	}

	mounted(): void {
		if (WebGL.isWebGLAvailable()) {
			Log.info('Wave effect created!');
			this.vantaEffect = WAVES({
				THREE,
				el: '.background-effect',
				mouseControls: true,
				touchControls: true,
				gyroControls: false,
				minHeight: 200.0,
				minWidth: 200.0,
				scale: 1.0,
				scaleMobile: 1.0,
				color: 0x880000,
				shininess: 43.0,
				waveHeight: 4.0,
				waveSpeed: 1.25,
				zoom: 0.65,
			});
		}
	}

	beforeDestroy(): void {
		if (this.vantaEffect) {
			Log.info('Wave effect destroyed!');
			this.vantaEffect.destroy();
		}
	}
}
</script>

<style lang="scss">
@import 'assets/scss/_variables.scss';

.background-effect,
.background-overlay {
	position: fixed;
	width: 100%;
	height: 100%;
}

.background-effect {
	z-index: -1 !important;

	.vanta-canvas {
		position: fixed;
	}

	&.still-background-effect {
		background-image: url('~assets/img/background/background.png');
	}
}

.background-overlay {
	&.dark-background {
		background-color: $dark-background-color;
	}

	&.light-background {
		background-color: $light-background-color;
	}
}
</style>
