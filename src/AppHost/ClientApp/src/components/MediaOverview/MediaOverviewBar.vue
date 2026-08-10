<template>
	<div class="media-overview-bar-container">
	<q-toolbar class="media-overview-bar">
		<!--	Title	-->
		<q-toolbar-title style="overflow: visible">
			<QRow
				align="center"
				justify="start">
				<Transition
					appear
					enter-active-class="animated fadeInLeft"
					leave-active-class="animated fadeOutLeft">
					<QCol
						v-if="detailMode"
						cols="auto">
						<q-btn
							flat
							icon="mdi-arrow-left"
							size="xl"
							@click="$emit('action', 'back')" />
					</QCol>
				</Transition>
				<QCol cols="auto">
					<MediaOverviewBarHeader
						:library-id="libraryId"
						:detail-mode="detailMode"
						:media-detail-item="mediaDetailItem" />
				</QCol>
				<!-- Search Bar -->
				<QCol align-self="center">
					<MediaOverviewSearchBar :library-id="libraryId" />
				</QCol>
			</QRow>
		</q-toolbar-title>

		<!--	Download button	-->
		<VerticalButton
			v-if="mediaOverviewStore.showDownloadButton"
			:height="barHeight"
			:label="$t('general.commands.download')"
			:width="verticalButtonWidth"
			icon="mdi-download"
			cy="media-overview-bar-download-button"
			@click="downloadCommandBus.emit('download')" />

		<!--	Selection Dialog Button	-->
		<VerticalButton
			v-if="mediaOverviewStore.showSelectionButton"
			:height="barHeight"
			:label="$t('general.commands.selection')"
			:width="verticalButtonWidth"
			icon="mdi-select-marker"
			@click="$emit('action', 'selection-dialog')" />

		<!--	Sort button	-->
		<VerticalButton
			v-if="!detailMode"
			:height="barHeight"
			:icon="activeSortIcon"
			:label="$t('general.commands.sort')"
			:width="verticalButtonWidth"
			:color="mediaOverviewStore.getIsSorted ? 'positive' : undefined"
			cy="media-overview-sort-btn">
			<q-menu
				anchor="bottom left"
				auto-close
				self="top left">
				<q-list>
					<!--	Clear Sort	-->
					<q-item
						v-if="mediaOverviewStore.getIsSorted"
						clickable
						cy="sort-clear-btn"
						@click="useSubscription(mediaOverviewStore.clearSort().subscribe())">
						<q-item-section avatar>
							<q-icon name="mdi-sort-variant-remove" />
						</q-item-section>
						<q-item-section>{{ $t('general.sort.clear') }}</q-item-section>
					</q-item>
					<q-separator v-if="mediaOverviewStore.getIsSorted" />
					<!--	Sort options	-->
					<q-item
						v-for="option in mediaOverviewStore.getSortOptions()"
						:key="option.field"
						:data-cy="`sort-option-${option.field}-btn`"
						clickable
						style="min-width: 200px"
						@click="mediaOverviewStore.toggleSortMedia(option.field)">
						<q-item-section avatar>
							<q-icon
								v-if="option.direction !== SortDirection.NoSort"
								:name="option.direction === SortDirection.Asc ? 'mdi-arrow-up' : 'mdi-arrow-down'" />
						</q-item-section>
						<q-item-section>{{ option.label }}</q-item-section>
					</q-item>
				</q-list>
			</q-menu>
		</VerticalButton>

		<!--	Refresh library button	-->
		<VerticalButton
			v-if="!mediaOverviewStore.allMediaMode && !detailMode"
			:height="barHeight"
			:label="$t('general.commands.refresh')"
			:width="verticalButtonWidth"
			cy="media-overview-refresh-library-btn"
			icon="mdi-refresh"
			@click="$emit('action', 'refresh-library');" />

		<!--	Media Options button	-->
		<VerticalButton
			v-if="mediaOverviewStore.allMediaMode && !detailMode"
			:height="barHeight"
			:label="$t('general.commands.media-options')"
			:width="verticalButtonWidth"
			cy="media-overview-options-btn"
			icon="mdi-tune"
			@click="$emit('action', 'media-options-dialog');" />

		<!--	View mode	-->
		<VerticalButton
			v-if="!detailMode"
			:height="barHeight"
			:label="$t('general.commands.view')"
			:width="verticalButtonWidth"
			cy="change-view-mode-btn"
			icon="mdi-eye">
			<q-menu
				anchor="bottom left"
				auto-close
				self="top left">
				<q-list>
					<q-item
						v-for="(viewOption, i) in viewOptions"
						:key="i"
						:data-cy="`view-mode-${viewOption.viewMode.toLowerCase()}-btn`"
						clickable
						style="min-width: 200px"
						@click="changeView(viewOption.viewMode)">
						<!-- View mode options -->
						<q-item-section avatar>
							<q-icon
								v-if="isSelected(viewOption.viewMode)"
								name="mdi-check" />
						</q-item-section>
						<!--	Is selected icon	-->
						<q-item-section> {{ viewOption.label }}</q-item-section>
					</q-item>
				</q-list>
			</q-menu>
		</VerticalButton>
	</q-toolbar>
	<q-banner
		v-if="isLibraryInaccessible"
		class="media-overview-inaccessible-banner bg-warning text-black"
		dense
		data-cy="media-overview-inaccessible-library-banner">
		<template #avatar>
			<q-icon name="mdi-alert" />
		</template>
		{{ $t('components.media-overview.library-inaccessible') }}
	</q-banner>
	</div>
</template>

<script lang="ts" setup>
import type { PlexMediaDTO } from '@dto';
import { ViewMode } from '@dto';
import { SortDirection } from '@enums';
import type { IMediaOverviewBarActions, IViewOptions } from '@interfaces';
import { useAccountStore, useMediaOverviewBarDownloadCommandBus, useMediaOverviewStore, useSettingsStore } from '#imports';

const mediaOverviewStore = useMediaOverviewStore();
const accountStore = useAccountStore();
const downloadCommandBus = useMediaOverviewBarDownloadCommandBus();

const settingsStore = useSettingsStore();

const props = withDefaults(defineProps<{
	libraryId?: number;
	detailMode?: boolean;
	mediaDetailItem?: PlexMediaDTO | null;
}>(), {
	libraryId: 0,
	detailMode: false,
});

const isLibraryInaccessible = computed(() =>
	props.libraryId > 0 && !accountStore.getHasAccountLibraryAccess(props.libraryId),
);

defineEmits<{
	(e: 'action', payload: IMediaOverviewBarActions): void;
}>();

const barHeight = ref(85);
const verticalButtonWidth = ref(120);

function isSelected(viewMode: ViewMode) {
	return mediaOverviewStore.getMediaViewMode === viewMode;
}

const activeSortIcon = computed((): string => {
	if (!mediaOverviewStore.getIsSorted) {
		return 'mdi-sort';
	}
	const sort = mediaOverviewStore.getActiveSort.sort;
	return sort === SortDirection.Asc ? 'mdi-sort-descending' : 'mdi-sort-ascending';
});

const viewOptions = computed((): IViewOptions[] => {
	return [
		{
			label: 'Poster View',
			viewMode: ViewMode.Poster,
		},
		{
			label: 'Table View',
			viewMode: ViewMode.Table,
		},
	];
});

function changeView(viewMode: ViewMode) {
	settingsStore.updateDisplayMode(mediaOverviewStore.getMediaType, viewMode);
}
</script>

<style lang="scss">
@use '@/assets/scss/variables' as *;
@use '@/assets/scss/mixins';

.media-overview-bar {
  @extend .fade-out-border;

  height: $media-overview-bar-height;
}

.media-overview-inaccessible-banner {
  border-top: 1px solid rgb(0 0 0 / 18%);
}

.q-fab__label {
  max-height: none;
}
</style>
