<template>
	<q-input
		v-model="mediaOverviewStore.filterQuery"
		:debounce="300"
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
			<q-chip
				v-for="(chip, index) in mediaOverviewStore.getFilterChips"
				:key="index"
				removable
				outline
				@remove="unsetMetaData(chip.key)">
				{{ chip.text }}
			</q-chip>
			<q-icon
				v-if="mediaOverviewStore.filterQuery !== ''"
				name="mdi-close"
				class="cursor-pointer q-mr-sm"
				@click="mediaOverviewStore.clearFilter()" />
		</template>
	</q-input>
</template>

<script setup lang="ts">
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
</script>
