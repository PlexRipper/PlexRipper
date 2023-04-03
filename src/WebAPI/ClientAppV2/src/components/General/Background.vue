<template>
	<div :class="backgroundEffect">
		<div :class="backgroundOverlay">
			<slot />
		</div>
	</div>
</template>

<script setup lang="ts">
import Log from 'consola';
import { defineProps, computed, onBeforeUnmount, onMounted, ref } from 'vue';
import * as THREE from 'three';
import WAVES from 'vanta/dist/vanta.waves.min';
import WebGL from '@class/WebGL';

const $q = useQuasar();

const vantaEffect = ref(null);

const props = defineProps<{ hideBackground: boolean }>();

const isDark = computed(() => {
	return $q.dark.isActive;
});

const backgroundEffect = computed(() => {
	return {
		'background-effect': true,
		'still-background-effect': !vantaEffect.value,
	};
});

const backgroundOverlay = computed(() => {
	if (props.hideBackground) {
		return {
			'background-overlay': true,
			'no-background': true,
		};
	}
	return {
		'background-overlay': true,
		'dark-background': isDark.value,
		'light-background': !isDark.value,
	};
});

onMounted(() => {
	if (WebGL.isWebGLAvailable()) {
		Log.info('Wave effect created!');
		vantaEffect.value = WAVES({
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
});

onBeforeUnmount(() => {
	if (vantaEffect.value) {
		Log.info('Wave effect destroyed!');
		// @ts-ignore
		vantaEffect.value.destroy();
	}
});
</script>

<style lang="scss">
.background-effect,
.background-overlay {
	position: fixed;
	width: 100%;
	height: 100%;
	top: 0;
}

.background-effect {
	z-index: -1 !important;

	.vanta-canvas {
		position: fixed;
	}

	&.still-background-effect {
		background-image: url('/img/background/background.png');
	}
}
</style>
