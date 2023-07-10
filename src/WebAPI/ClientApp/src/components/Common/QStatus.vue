<template>
	<q-tooltip>
		{{ value ? $t('components.q-status.server-connectable') : $t('components.q-status.server-unconnectable') }}
	</q-tooltip>
	<span v-if="pulse" class="status-indicator" :[status]="true" pulse />
	<span v-else class="status-indicator" :[status]="true" />
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
<style lang="scss">
.status-indicator {
	margin-bottom: 2px;
}
</style>
