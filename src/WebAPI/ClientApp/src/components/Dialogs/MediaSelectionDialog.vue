<template>
	<QCardDialog
		min-width="50vw"
		max-width="50vw"
		:name="name"
		:loading="false"
		@opened="onOpen()"
		@closed="onClose"
	>
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
			<QRow
				justify="center"
				align="center"
				class="q-pt-lg"
			>
				<QCol
					cols="11"
					class="q-my-md"
				>
					<q-range
						v-model="selectedRange"
						:min="1"
						:max="mediaOverviewStore.itemsLength"
						:step="1"
						track-size="5px"
						thumb-size="35px"
						label-always
						drag-range
						color="red"
					/>
				</QCol>
			</QRow>
			<QRow justify="between">
				<QCol
					v-for="column in ['min', 'max']"
					:key="column"
					cols="auto"
					class="q-mx-xs"
				>
					<table>
						<tr>
							<td colspan="2">
								<q-input
									v-model.number="numberInput[column]"
									type="number"
									outlined
									style="max-width: 200px"
								/>
							</td>
						</tr>
						<tr
							v-for="index in [1, 10, 100, 1000, 10000]"
							:key="index"
						>
							<td>
								<BaseButton
									:label="`-${index}`"
									block
									@click="adjustValue(column, -1 * index)"
								/>
							</td>
							<td>
								<BaseButton
									:label="`+${index}`"
									block
									@click="adjustValue(column, index)"
								/>
							</td>
						</tr>
					</table>
				</QCol>
			</QRow>
		</template>
		<!--	Close action	-->
		<template #actions="{ close }">
			<QRow justify="between">
				<QCol cols="2">
					<BaseButton
						:label="$t('general.commands.close')"
						block
						@click="close"
					/>
				</QCol>
				<QCol cols="2">
					<BaseButton
						:label="$t('general.commands.set-selection')"
						color="positive"
						block
						@click="setSelection"
					/>
				</QCol>
			</QRow>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
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
		adjustValue('min', value.min);
		adjustValue('max', value.max);
	},
});

function adjustValue(type: string, value: number) {
	let minValue = 1;
	let maxValue = mediaOverviewStore.itemsLength;
	switch (type) {
		case 'min':
			minValue = 1;
			maxValue = get(selectedRange).max;
			break;
		case 'max':
			minValue = get(selectedRange).min;
			maxValue = mediaOverviewStore.itemsLength;
			break;
	}
	get(selectedRange)[type] = clamp(get(selectedRange)[type] + value, minValue, maxValue);
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
