<template>
	<q-input
		:model-value="mediaOverviewStore.filterQuery"
		:debounce="100"
		outlined
		input-style="font-size: 1.25rem"
		rounded
		@update:model-value="(value) => useSubscription(mediaOverviewStore.setFilterQuery(String(value ?? '')).subscribe())">
		<template #prepend>
			<IconButton
				icon="mdi-magnify">
				<MediaFilterMenu :library-id="libraryId" />
			</IconButton>
		</template>
		<template #append>
			<QGlowChip
				v-for="chip in mediaOverviewStore.getFilterChips"
				:key="chip.id"
				:value="chip.text"
				:color="chip.color"
				removable
				@remove="useSubscription(chip.unset.subscribe())" />
			<q-icon
				v-if="mediaOverviewStore.filterQuery !== ''"
				name="mdi-close"
				class="cursor-pointer q-mr-sm"
				@click="useSubscription(mediaOverviewStore.clearFilter().subscribe())" />
		</template>
	</q-input>
</template>

<script setup lang="ts">
import { useMediaOverviewStore } from '@store';

const mediaOverviewStore = useMediaOverviewStore();

withDefaults(defineProps<{
	libraryId?: number;
}>(), {
	libraryId: 0,
});
</script>
