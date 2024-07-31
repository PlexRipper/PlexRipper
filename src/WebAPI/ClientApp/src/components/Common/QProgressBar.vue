<template>
	<q-linear-progress :value="value / 100">
		<div class="absolute-full flex flex-center q-progress-bar-text">
			<strong>{{ $n(Math.round(value * 100) / 100, 'percent') }}</strong>
		</div>
	</q-linear-progress>
</template>

<script setup lang="ts">
defineProps<{
	value: number;
}>();
</script>

<style lang="scss">
.q-progress-bar-text {
	font-size: 0.8rem;
}
</style>
