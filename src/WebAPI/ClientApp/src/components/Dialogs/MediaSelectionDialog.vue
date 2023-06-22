<template>
	<QCardDialog min-width="50vw" max-width="50vw" :name="name" :loading="false" @opened="onOpen()" @closed="onClose">
		<template #title>
			{{
				$t('components.media-selection-dialog.title', {
					min: selectedRange.min,
					max: selectedRange.max,
				})
			}}
		</template>
		<!--	Help text	-->
		<template #default>
			<q-row justify="center" align="center" class="q-pt-lg">
				<q-col cols="11" class="q-my-md">
					<q-range
						v-model="selectedRange"
						:min="1"
						:max="mediaOverviewStore.itemsLength"
						:step="1"
						track-size="5px"
						thumb-size="35px"
						label-always
						drag-range
						color="red" />
				</q-col>
			</q-row>
			<q-row justify="between">
				<q-col v-for="column in ['min', 'max']" :key="column" cols="auto" class="q-mx-xs">
					<table>
						<tr>
							<td colspan="2">
								<q-input v-model.number="numberInput[column]" type="number" outlined style="max-width: 200px" />
							</td>
						</tr>
						<tr v-for="index in [1, 10, 100, 1000, 10000]" :key="index">
							<td>
								<base-button :label="`-${index}`" block @click="adjustValue(column, -1 * index)" />
							</td>
							<td>
								<base-button :label="`+${index}`" block @click="adjustValue(column, index)" />
							</td>
						</tr>
					</table>
				</q-col>
			</q-row>
		</template>
		<!--	Close action	-->
		<template #actions="{ close }">
			<q-row justify="between">
				<q-col cols="2">
					<base-button text-id="close" block @click="close" />
				</q-col>
				<q-col cols="2">
					<base-button text-id="set-selection" color="positive" block @click="setSelection" />
				</q-col>
			</q-row>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import { clamp } from 'lodash-es';
import { useMediaOverviewStore } from '#imports';

const mediaOverviewStore = useMediaOverviewStore();

defineProps<{ name: string }>();

const selectedRange = ref({
	min: 1,
	max: 1,
});

const numberInput = computed({
	get: () => selectedRange.value,
	set: (value) => {
		selectedRange.value = {
			min: clamp(value.min, 1, mediaOverviewStore.itemsLength),
			max: clamp(value.max, 1, mediaOverviewStore.itemsLength),
		};
	},
});

function adjustValue(type: string, value: number) {
	get(selectedRange)[type] = clamp(get(selectedRange)[type] + value, 1, mediaOverviewStore.itemsLength);
}

function setSelection() {
	mediaOverviewStore.setSelectionRange(selectedRange.value.min, selectedRange.value.max);
}
function onOpen(): void {
	set(selectedRange, {
		min: 1,
		max: mediaOverviewStore.itemsLength,
	});
}
function onClose() {}
</script>
