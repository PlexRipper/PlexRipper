<template>
	<q-input v-model="model" :outlined="props.outlined" @keyup.enter="$emit('click:append')">
		<template v-if="props.appendIcon" #append>
			<q-btn :icon="appendIcon" flat @click="$emit('click:append')" />
		</template>
	</q-input>
	{{ model }}
</template>

<script setup lang="ts">
const model = defineModel<string>();
const props = withDefaults(
	defineProps<{
		outlined?: boolean;
		appendIcon?: string;
	}>(),
	{
		outlined: true,
		appendIcon: '',
	},
);

defineEmits<{ (e: 'click:append'): void }>();
</script>
