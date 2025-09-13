<template>
	<q-linear-progress
		:value="value / 100"
		:data-cy="cy">
		<div class="absolute-full flex flex-center q-progress-bar-text">
			<strong>{{ n(value / 100, 'percent') }}</strong>
		</div>
	</q-linear-progress>
</template>

<script setup lang="ts">
defineProps<{
	value: number;
	cy?: string;
}>();

const { n } = useI18n();
</script>

<style lang="scss">
.q-progress-bar-text {
  font-size: 0.8rem;
}
</style>
