<template>
	<q-icon
		:name="icon"
		:size="size + 'px'" />
</template>

<script setup lang="ts">
import Convert from '@class/Convert';
import type { PlexMediaType } from '@dto';

const props = withDefaults(defineProps<{ mediaType: string; size?: number }>(), {
	mediaType: '',
	size: 24,
});

const icon = computed((): string => {
	return Convert.mediaTypeToIcon(props.mediaType as PlexMediaType);
});
</script>
