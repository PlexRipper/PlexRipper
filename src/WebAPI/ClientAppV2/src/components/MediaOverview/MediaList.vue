<template>
	<div>
		{{ selected }}
		{{ itemExpanded }}
		<q-list v-if="mediaItem?.children?.length > 0" bordered class="rounded-borders">
			<q-item>
				<q-item-section avatar>
					<q-checkbox :model-value="rootSelected" @update:model-value="rootSetSelected($event)" />
				</q-item-section>
				<q-item-section avatar>
					<q-sub-header class="q-pl-none">
						{{ mediaItem.title }}
					</q-sub-header>
				</q-item-section>
			</q-item>

			<q-expansion-item
				v-for="(child, index) in mediaItem.children"
				:key="child.id"
				:model-value="itemExpanded[index]"
				expand-separator
				icon="perm_identity"
				hide-expand-icon
				:default-opened="defaultOpened"
				:group="defaultOpened ? undefined : 'media-list'"
				:label="child.title"
				@update:model-value="itemExpanded[index] = $event">
				<!-- Header	-->
				<template #header="{ expanded }">
					<q-row align="center">
						<q-col cols="auto">
							<q-checkbox
								:model-value="isSelected(child.id)"
								@update:model-value="setSelected(child.id, child.children, $event)" />
						</q-col>
						<q-col class="q-ml-md">
							<q-sub-header>
								{{ child.title }}
							</q-sub-header>
						</q-col>
						<q-col cols="auto">
							<q-icon size="lg" :name="expanded ? 'mdi-chevron-up' : 'mdi-chevron-down'" />
						</q-col>
					</q-row>
				</template>
				<!-- Body	-->
				<template #default>
					<MediaTable
						:loading="loading"
						:rows="child.children"
						:selected="getSelected(child.id)"
						row-key="id"
						@selection="onSelection(child.id, $event)" />
				</template>
			</q-expansion-item>
		</q-list>
		<q-list v-else>
			<q-item>
				<q-item-section><h2>No media found</h2></q-item-section>
			</q-item>
		</q-list>
	</div>
</template>

<script setup lang="ts">
import { ref, defineProps, withDefaults } from 'vue';
import { watchOnce } from '@vueuse/core';
import { PlexMediaDTO } from '@dto/mainApi';
import ISelection from '@interfaces/ISelection';

const defaultOpened = ref(false);

const props = withDefaults(
	defineProps<{
		mediaItem: PlexMediaDTO | null;
		loading?: boolean;
	}>(),
	{
		mediaItem: null,
		loading: false,
	},
);

const selected = ref<ISelection[]>([]);
const itemExpanded = ref<boolean[]>([]);

const rootSelected = computed((): boolean | null => {
	const allSelected = selected.value.map((x) => x.allSelected);
	if (allSelected.every((x) => x === true)) {
		return true;
	}

	if (allSelected.every((x) => x === false)) {
		return false;
	}

	return null;
});

const rootSetSelected = (value: boolean) => {
	for (const child of props.mediaItem?.children ?? []) {
		setSelected(child.id, child.children, value);
	}
};

const isSelected = (id: number): boolean | null => {
	if (selected.value.length === 0) {
		return false;
	}
	const result = selected.value.find((x) => x.indexKey === id);
	if (result === undefined) {
		return false;
	}
	return result.allSelected;
};

const getSelected = (id: number): string[] | number[] => {
	if (selected.value.length === 0) {
		return [];
	}
	const result = selected.value.find((x) => x.indexKey === id);
	if (result === undefined) {
		return [];
	}
	return result.keys;
};

const setSelected = (id: number, children: PlexMediaDTO[], value: boolean) => {
	const selection = selected.value.find((x) => x.indexKey === id);
	if (selection) {
		if (value) {
			selection.allSelected = true;
			selection.keys = children.map((x) => x.id);
		} else {
			selection.allSelected = false;
			selection.keys = [];
		}
	}
};

const onSelection = (id: number, { allSelected, selection }: { allSelected: boolean | null; selection: string[] }) => {
	const i = selected.value.findIndex((x) => x.indexKey === id);
	if (i === -1) {
		selected.value.push({ indexKey: id, keys: selection, allSelected });
		return;
	}

	selected.value[i].allSelected = allSelected;
	selected.value[i].keys = selection;
};

const expandAll = () => {
	defaultOpened.value = true;
};

onMounted(() => {
	// triggers only once
	const children = props.mediaItem?.children ?? [];
	if (children.length === 0) {
		return;
	}

	selected.value = children.map((x) => ({ indexKey: x.id, keys: [], allSelected: false })) ?? [];
	itemExpanded.value.fill(false, 0, children.length - 1);
});

defineExpose({
	expandAll,
});
</script>
