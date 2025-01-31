<template>
	<span
		v-if="pulse"
		v-bind="$attrs"
		class="status-indicator"
		:[status]="true"
		pulse />
	<span
		v-else
		v-bind="$attrs"
		class="status-indicator"
		:[status]="true" />
	<q-tooltip>
		{{ value ? $t('components.q-status.server-connectable') : $t('components.q-status.server-unconnectable') }}
	</q-tooltip>
</template>

<script setup lang="ts">
// Stolen from: https://www.npmjs.com/package/vue-status-indicator
const props = defineProps<{ value: boolean }>();

const status = computed(() => {
	return props.value ? 'positive' : 'negative';
});

const pulse = computed(() => {
	return props.value;
});
</script>
