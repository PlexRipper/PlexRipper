<template>
	<div :class="backgroundOverlay">
		<slot />
	</div>
</template>

<script setup lang="ts">
const $q = useQuasar();

const props = withDefaults(defineProps<{ hideBackground?: boolean }>(), {
	hideBackground: false,
});

const isDark = computed(() => {
	return $q.dark.isActive;
});

const vantaEffect = computed(() => {
	// @ts-ignore
	return window?.waveEffect ?? null;
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
</script>

<style lang="scss">
.background-overlay {
	position: fixed;
	width: 100%;
	height: 100%;
	top: 0;
	z-index: -1;
}
</style>
