<template>
	<q-input
		:model-value="mediaOverviewStore.filterQuery"
		:debounce="300"
		@update:model-value="onFilterQueryChanged"
		outlined
		input-style="font-size: 1.25rem"
		rounded>
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
				@remove="unsetMetaData(chip.key)" />
			<q-icon
				v-if="mediaOverviewStore.filterQuery !== ''"
				name="mdi-close"
				class="cursor-pointer q-mr-sm"
				@click="mediaOverviewStore.clearFilter()" />
		</template>
	</q-input>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { useMediaOverviewStore } from '@store';
import IconButton from '@components/Buttons/IconButton.vue';
import type { IMetaDataMediaFilter } from '@interfaces';

const mediaOverviewStore = useMediaOverviewStore();

withDefaults(defineProps<{
	libraryId?: number;
}>(), {
	libraryId: 0,
});

function unsetMetaData(key: keyof IMetaDataMediaFilter) {
	useSubscription(mediaOverviewStore.unsetMetaData(key).subscribe());
}

function onFilterQueryChanged(value: string | number | null) {
	mediaOverviewStore.setFilterQuery(String(value ?? ''));
	useSubscription(mediaOverviewStore.requestMediaFirstPage().subscribe());
}
</script>
