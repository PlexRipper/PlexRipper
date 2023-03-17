<template>
	<span class="status-indicator" v-if="pulse" :[status]="true" pulse/>
	<span class="status-indicator" v-else :[status]="true"/>
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
