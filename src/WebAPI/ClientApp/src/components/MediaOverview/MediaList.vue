<template>
	<q-list v-if="mediaItem?.children?.length ?? -1 > 0">
		<!--	Root item	-->
		<q-item>
			<q-row align="center">
				<q-col cols="auto">
					<q-checkbox :model-value="rootSelected" @update:model-value="rootSetSelected($event)" />
				</q-col>
				<q-col class="q-ml-md">
					<q-sub-header bold>
						{{ mediaItem?.title ?? t('general.error.unknown') }}
					</q-sub-header>
				</q-col>
				<!--	Total selected count	-->
				<q-col v-if="selectedCount" cols="auto">
					<span class="text-weight-bold">
						{{ t('components.media-list.selected-count', { selectedCount }) }}
					</span>
				</q-col>
			</q-row>
		</q-item>
		<!-- Season display -->
		<q-expansion-item
			v-for="(child, index) in mediaItem?.children ?? []"
			:key="child.id"
			:model-value="itemExpanded[index]"
			expand-separator
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
						<q-sub-header bold>
							{{ child.title }}
							<span class="text-weight-light q-ml-md">
								{{ t('components.media-list.episode-count', { episodeCount: child.childCount }) }}
							</span>
						</q-sub-header>
					</q-col>
					<q-col v-if="getSelected(child.id)?.keys.length" cols="auto">
						<span class="text-weight-bold">
							{{
								t('components.media-list.selected-count', {
									selectedCount: getSelected(child.id)?.keys.length ?? -1,
								})
							}}
						</span>
					</q-col>
					<q-col cols="auto">
						<q-icon size="lg" :name="expanded ? 'mdi-chevron-up' : 'mdi-chevron-down'" />
					</q-col>
				</q-row>
			</template>
			<!-- Body	-->
			<template #default>
				<MediaQTable
					v-if="useQTable"
					:rows="child.children"
					:selection="getSelected(child.id)"
					@selection="onSelection(child.id, $event)" />
				<MediaTable
					v-else
					:rows="child.children"
					:selection="getSelected(child.id)"
					row-key="id"
					disable-hover-click
					:disable-highlight="disableHighlight"
					:disable-intersection="disableIntersection"
					@selection="onSelection(child.id, $event)" />
			</template>
		</q-expansion-item>
	</q-list>
	<q-list v-else>
		<q-item>
			<q-item-section>
				<h2>{{ t('components.media-list.no-media-found') }}</h2>
			</q-item-section>
		</q-item>
	</q-list>
</template>

<script setup lang="ts">
import Log from 'consola';
import { get, set } from '@vueuse/core';
import { DownloadMediaDTO, PlexMediaSlimDTO, PlexMediaType } from '@dto/mainApi';
import ISelection from '@interfaces/ISelection';
import {
	useMediaOverviewBarBus,
	useMediaOverviewBarDownloadCommandBus,
	toDownloadMedia,
	sendMediaOverviewDownloadCommand,
} from '#imports';

const defaultOpened = ref(false);
const { t } = useI18n();
const props = withDefaults(
	defineProps<{
		mediaItem: PlexMediaSlimDTO | null;
		loading?: boolean;
		disableIntersection: boolean;
		disableHighlight: boolean;
		useQTable: boolean;
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

// region Selection

const selectedCount = computed((): number => {
	return selected.value?.reduce((acc, x) => acc + x.keys?.length ?? 0, 0) ?? 0;
});

function rootSetSelected(value: boolean) {
	for (const child of props.mediaItem?.children ?? []) {
		setSelected(child.id, child.children, value);
	}
}

function isSelected(id: number): boolean | null {
	if (selected.value.length === 0) {
		return false;
	}
	const result = selected.value.find((x) => x.indexKey === id);
	if (result === undefined) {
		return false;
	}
	return result.allSelected;
}

function getSelected(id: number): ISelection | null {
	if (selected.value.length === 0) {
		return null;
	}
	const result = selected.value.find((x) => x.indexKey === id);
	if (result === undefined) {
		return null;
	}
	return result;
}

function setSelected(id: number, children: PlexMediaSlimDTO[], value: boolean) {
	const selection = get(selected).find((x) => x.indexKey === id);
	if (selection) {
		if (value) {
			selection.allSelected = true;
			selection.keys = children.map((x) => x.id);
		} else {
			selection.allSelected = false;
			selection.keys = [];
		}
	}
}

function onSelection(id: number, payload: ISelection) {
	const i = selected.value.findIndex((x) => x.indexKey === id);
	if (i === -1) {
		selected.value.push({ indexKey: id, keys: payload.keys, allSelected: payload.allSelected });
		return;
	}

	selected.value[i].allSelected = payload.allSelected;
	selected.value[i].keys = payload.keys;
}

// endregion

function expandAll() {
	defaultOpened.value = true;
}

// region EventBus
const mediaOverViewBarBus = useMediaOverviewBarBus();

function sendBusConfig() {
	mediaOverViewBarBus.emit({
		downloadButtonVisible: get(selectedCount) > 0,
		hasSelected: get(selectedCount) > 0,
	});
}

watch(selectedCount, () => {
	sendBusConfig();
});

useMediaOverviewBarDownloadCommandBus().on(() => {
	if (!props.mediaItem) {
		Log.error('No media item selected');
		return;
	}

	const mediaItem = props.mediaItem;

	// All selected thus entire TvShow
	if (rootSelected.value) {
		sendMediaOverviewDownloadCommand(toDownloadMedia(mediaItem));
		return;
	}

	const downloadMedia: DownloadMediaDTO[] = [];
	const seasonIds: number[] = [];
	const episodesIds: number[] = [];

	for (const selection of get(selected)) {
		if (selection.allSelected) {
			seasonIds.push(selection.indexKey);
		} else {
			episodesIds.push(...selection.keys.map((x) => +x));
		}
	}

	if (seasonIds.length > 0) {
		downloadMedia.push({
			type: PlexMediaType.Season,
			mediaIds: seasonIds,
			plexLibraryId: mediaItem.plexLibraryId,
			plexServerId: mediaItem.plexServerId,
		});
	}

	if (episodesIds.length > 0) {
		downloadMedia.push({
			type: PlexMediaType.Episode,
			mediaIds: episodesIds,
			plexLibraryId: mediaItem.plexLibraryId,
			plexServerId: mediaItem.plexServerId,
		});
	}

	sendMediaOverviewDownloadCommand(downloadMedia);
});
// endregion
onMounted(() => {
	const children = props.mediaItem?.children ?? [];
	if (children.length === 0) {
		return;
	}

	set(selected, children.map((x) => ({ indexKey: x.id, keys: [], allSelected: false })) ?? []);
	get(itemExpanded).fill(false, 0, children.length - 1);
});

defineExpose({
	expandAll,
	sendBusConfig,
});
</script>
