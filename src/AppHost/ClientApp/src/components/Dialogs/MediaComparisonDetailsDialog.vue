<template>
	<QCardDialog
		:name="DialogType.MediaComparisonDetailsDialog"
		content-height="80"
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
						show-tooltip
						:comparison-state="getPlexMediaComparisonState(selectedMediaItem)"
						dense
						cy="media-comparison-details-dialog-state" />
				</QCol>
				<QCol>
					<QText size="h4">
						{{ selectedMediaItem?.title ?? t('general.error.unknown') }}
					</QText>
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
					<!-- Owned Quality	-->
					<template #body-cell-ownedQuality="{ row }: { row: IComparisonDetailRow }">
						<q-td class="text-eclipse">
							<MediaVideoQuality :quality="row.ownedQuality" />
						</q-td>
					</template>
					<!-- Remote Quality	-->
					<template #body-cell-remoteQuality="{ row }: { row: IComparisonDetailRow }">
						<q-td class="text-eclipse">
							<MediaVideoQuality :quality="row.remoteQuality" />
						</q-td>
					</template>
					<template #body-cell-reason="{ row } : {row: IComparisonDetailRow }">
						<q-td>
							<MediaComparisonStateButton
								:comparison-state="row.comparisonState"
								show-tooltip
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
import type { PlexMediaComparisonDetailsRowDTO, PlexMediaSlimDTO } from '@dto';
import { DialogType } from '@enums';
import { useMediaStore } from '@store';
import { getPlexMediaComparisonState, getPlexMediaComparisonStateFromId } from '@composables';

interface IComparisonDetailRow {
	key: string;
	title: string;
	remoteQuality: VideoQuality;
	ownedQuality: VideoQuality;
	remoteLocation: string;
	ownedLocation: string;
	comparisonState: PlexMediaComparisonState;
	level: number;
}

const { t } = useI18n();
const mediaStore = useMediaStore();

const loading = ref(false);
const comparisonRows = ref<PlexMediaComparisonDetailsRowDTO[]>([]);
const selectedMediaItem = ref<PlexMediaSlimDTO | null>(null);

const columns: QTableColumn[] = [
	{
		name: 'title',
		label: t('components.media-overview.comparison.details-column-title'),
		field: 'title',
		align: 'left',
	},
	{
		name: 'ownedQuality',
		label: t('components.media-overview.comparison.details-column-owned-quality'),
		field: 'ownedQuality',
		align: 'left',
	},
	{
		name: 'remoteQuality',
		label: t('components.media-overview.comparison.details-column-remote-quality'),
		field: 'remoteQuality',
		align: 'left',
	},
	{
		name: 'remoteLocation',
		label: t('components.media-overview.comparison.details-column-remote-location'),
		field: 'remoteLocation',
		align: 'left',
	},
	{
		name: 'ownedLocation',
		label: t('components.media-overview.comparison.details-column-owned-location'),
		field: 'ownedLocation',
		align: 'left',
	},
	{
		name: 'reason',
		label: t('components.media-overview.comparison.details-column-reason'),
		field: 'comparisonState',
		align: 'left',
	},
];

const detailRows = computed(() => {
	return get(comparisonRows)
		.filter((row) => row.isActionable)
		.map((row): IComparisonDetailRow => ({
			key: `${row.type}-${row.id}`,
			title: row.title,
			ownedQuality: row.ownedQuality ?? VideoQuality.None,
			remoteQuality: row.remoteQuality ?? VideoQuality.None,
			remoteLocation: locationLabel(row.remoteLocation, row.remoteLibraryTitle),
			ownedLocation: locationLabel(row.ownedLocation, row.ownedLibraryTitle),
			comparisonState: getPlexMediaComparisonStateFromId(row.comparisonId),
			level: row.level,
		}));
});

function onOpen(value: unknown) {
	const mediaItem = value as PlexMediaSlimDTO;
	set(selectedMediaItem, mediaItem);
	set(loading, true);
	set(comparisonRows, []);
	useSubscription(
		mediaStore.getMediaComparisonDetails(mediaItem.id, mediaItem.type).subscribe({
			next: (details) => set(comparisonRows, details.rows),
			complete: () => set(loading, false),
			error: () => set(loading, false),
		}),
	);
}

function locationLabel(location: string, libraryTitle: string): string {
	if (!location && !libraryTitle)
		return t('general.error.unknown');

	if (!location)
		return libraryTitle;

	if (!libraryTitle)
		return location;

	return `${libraryTitle}: ${location}`;
}

function descriptionFor(mediaItem: PlexMediaSlimDTO): string {
	switch (getPlexMediaComparisonState(mediaItem)) {
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
