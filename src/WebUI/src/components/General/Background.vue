<template>
	<div id="background">
		<slot />
	</div>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import * as THREE from 'three';
import WAVES from 'vanta/dist/vanta.waves.min';

@Component
export default class Background extends Vue {
	vantaEffect: any;

	mounted(): void {
		Log.info('Wave effect created!');
		this.vantaEffect = WAVES({
			THREE,
			el: '#background',
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

	beforeDestroy(): void {
		if (this.vantaEffect) {
			Log.info('Wave effect destroyed!');
			this.vantaEffect.destroy();
		}
	}
}
</script>
