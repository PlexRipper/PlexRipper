<template>
	<QCardDialog
		:name="DialogType.MediaComparisonDetailsDialog"
		width="900px"
		content-height="60"
		:loading="loading"
		close-button
		cy="media-comparison-details-dialog"
		@opened="onOpen">
		<template #title>
			<QRow
				align="center"
				gutter="sm">
				<QCol cols="auto">
					<MediaComparisonStateButton
						v-if="selectedMediaItem"
						:comparison-state="selectedMediaItem.comparisonState"
						show-label
						dense
						cy="media-comparison-details-dialog-state" />
				</QCol>
				<QCol>
					{{ selectedMediaItem?.title ?? t('general.error.unknown') }}
				</QCol>
			</QRow>
		</template>

		<template #default>
			<div
				v-if="selectedMediaItem"
				class="media-comparison-details">
				<QText
					class="q-mb-md"
					:value="descriptionFor(selectedMediaItem)" />

				<q-table
					flat
					:data-cy="'media-comparison-details-table'"
					:rows="detailRows"
					:columns="columns"
					row-key="key"
					:rows-per-page-options="[0]"
					hide-pagination>
					<template #body-cell-title="scope">
						<q-td :props="scope">
							<span :class="`media-comparison-details__title--level-${scope.row.level}`">
								{{ scope.row.title }}
							</span>
						</q-td>
					</template>
					<template #body-cell-size="scope">
						<q-td :props="scope">
							<QFileSize :size="scope.row.size" />
						</q-td>
					</template>
					<template #body-cell-reason="scope">
						<q-td :props="scope">
							<MediaComparisonStateButton
								:comparison-state="scope.row.comparisonState"
								show-label
								dense />
						</q-td>
					</template>
					<template #no-data>
						<div class="full-width text-center q-pa-md">
							{{ t('components.media-overview.comparison.details-no-actionable-rows') }}
						</div>
					</template>
				</q-table>
			</div>
		</template>

		<template #actions="{ close }">
			<QRow justify="end">
				<QCol cols="auto">
					<BaseButton
						:label="t('general.commands.close')"
						cy="media-comparison-details-close-button"
						@click="close" />
				</QCol>
				<QCol cols="auto">
					<BaseButton
						:label="t('components.media-overview.comparison.download-selected')"
						:disabled="detailRows.length === 0"
						cy="media-comparison-details-download-selected-button" />
				</QCol>
			</QRow>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import type { QTableColumn } from 'quasar';
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import { PlexMediaComparisonState, VideoQuality } from '@dto';
import type { PlexMediaDTO, PlexMediaSlimDTO } from '@dto';
import { DialogType } from '@enums';
import { useMediaStore } from '@store';

interface IComparisonDetailRow {
	key: string;
	title: string;
	quality: string;
	size: number;
	location: string;
	comparisonState: PlexMediaComparisonState;
	level: number;
}

const { t } = useI18n();
const mediaStore = useMediaStore();

const loading = ref(false);
const mediaDetail = ref<PlexMediaDTO | null>(null);
const selectedMediaItem = ref<PlexMediaSlimDTO | null>(null);

const actionableStates = [
	PlexMediaComparisonState.Missing,
	PlexMediaComparisonState.HigherQuality,
	PlexMediaComparisonState.Partial,
	PlexMediaComparisonState.PartialAndHigherQuality,
];

const columns: QTableColumn[] = [
	{ name: 'title', label: t('components.media-overview.comparison.details-column-title'), field: 'title', align: 'left' },
	{ name: 'quality', label: t('components.media-overview.comparison.details-column-quality'), field: 'quality', align: 'left' },
	{ name: 'size', label: t('components.media-overview.comparison.details-column-size'), field: 'size', align: 'left' },
	{ name: 'location', label: t('components.media-overview.comparison.details-column-location'), field: 'location', align: 'left' },
	{ name: 'reason', label: t('components.media-overview.comparison.details-column-reason'), field: 'comparisonState', align: 'left' },
];

const detailRows = computed(() => {
	const detail = get(mediaDetail);
	if (!detail)
		return [];

	const rows: IComparisonDetailRow[] = [];
	addActionableRow(rows, detail, 0);
	for (const child of detail.children) {
		addActionableRow(rows, child, 1);
		for (const grandChild of child.children)
			addActionableRow(rows, grandChild, 2);
	}

	return rows;
});

function onOpen(value: unknown) {
	const mediaItem = value as PlexMediaSlimDTO;
	set(selectedMediaItem, mediaItem);
	set(loading, true);
	set(mediaDetail, null);
	useSubscription(
		mediaStore.getMediaDataDetailById(mediaItem.id, mediaItem.type).subscribe({
			next: (detail) => set(mediaDetail, detail),
			complete: () => set(loading, false),
			error: () => set(loading, false),
		}),
	);
}

function addActionableRow(rows: IComparisonDetailRow[], mediaItem: PlexMediaDTO, level: number) {
	if (!actionableStates.includes(mediaItem.comparisonState))
		return;

	rows.push({
		key: `${mediaItem.type}-${mediaItem.id}`,
		title: mediaItem.title,
		quality: qualityLabel(mediaItem),
		size: mediaItem.mediaSize,
		location: t('components.media-overview.comparison.details-library-location', { libraryId: mediaItem.plexLibraryId }),
		comparisonState: mediaItem.comparisonState,
		level,
	});
}

function qualityLabel(mediaItem: PlexMediaDTO): string {
	const qualities = mediaItem.qualities.map((x) => x.quality).filter((x) => x !== VideoQuality.Unknown);
	if (qualities.length === 0)
		return VideoQuality.Unknown;

	return [...new Set(qualities)].join(', ');
}

function descriptionFor(mediaItem: PlexMediaSlimDTO): string {
	switch (mediaItem.comparisonState) {
		case PlexMediaComparisonState.Missing:
			return t('components.media-overview.comparison.details-missing', { title: mediaItem.title });
		case PlexMediaComparisonState.HigherQuality:
			return t('components.media-overview.comparison.details-higher-quality', { title: mediaItem.title });
		case PlexMediaComparisonState.Partial:
			return t('components.media-overview.comparison.details-partial', { title: mediaItem.title });
		case PlexMediaComparisonState.PartialAndHigherQuality:
			return t('components.media-overview.comparison.details-partial-and-higher-quality', { title: mediaItem.title });
		default:
			return t('components.media-overview.comparison.details-not-actionable', { title: mediaItem.title });
	}
}
</script>

<style lang="scss">
.media-comparison-details__title--level-1 {
  padding-left: 1rem;
}

.media-comparison-details__title--level-2 {
  padding-left: 2rem;
}
</style>
