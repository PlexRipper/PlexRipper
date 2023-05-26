<template>
	<q-input :model-value="modelValue" :outlined="props.outlined">
		<template v-if="props.appendIcon" #append>
			<q-btn :icon="appendIcon" flat @click="$emit('click:append')" />
		</template>
	</q-input>
</template>

<script setup lang="ts">
const props = withDefaults(
	defineProps<{
		modelValue: string;
		outlined?: boolean;
		appendIcon?: string;
	}>(),
	{
		modelValue: '',
		outlined: true,
		appendIcon: '',
	},
);

defineEmits<{ (e: 'click:append'): void }>();
</script>
