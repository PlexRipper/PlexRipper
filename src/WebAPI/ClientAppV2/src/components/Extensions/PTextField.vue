<template>
	<q-input :outlined="props.outlined">
		<template v-if="props.appendIcon" #append>
			<q-btn :icon="appendIcon" flat @click="$emit('click:append')" />
		</template>
	</q-input>
</template>

<script setup lang="ts">
import { withDefaults, defineProps, defineEmits } from 'vue';

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
