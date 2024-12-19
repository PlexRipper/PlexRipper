<template>
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
						:media-type="mediaType"
						:all-media-mode="allMediaMode"
						:media-detail-item="mediaDetailItem" />
				</QCol>
				<!-- Search Bar -->
				<QCol align-self="center">
					<q-input
						v-model="mediaOverviewStore.filterQuery"
						:debounce="300"
						outlined
						input-style="font-size: 1.25rem"
						rounded>
						<template #prepend>
							<q-icon
								name="mdi-magnify"
								class="q-ml-sm" />
						</template>
						<template #append>
							<q-icon
								v-if="mediaOverviewStore.filterQuery !== ''"
								name="mdi-close"
								class="cursor-pointer q-mr-sm"
								@click="mediaOverviewStore.clearFilter()" />
						</template>
					</q-input>
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
</template>

<script lang="ts" setup>
import { ViewMode } from '@dto';
import type { PlexMediaDTO, PlexMediaType } from '@dto';
import type { IMediaOverviewBarActions, IViewOptions } from '@interfaces';
import {
	useMediaOverviewBarDownloadCommandBus,
	useMediaOverviewStore,
	useSettingsStore,
} from '#imports';

const mediaOverviewStore = useMediaOverviewStore();
const downloadCommandBus = useMediaOverviewBarDownloadCommandBus();

const settingsStore = useSettingsStore();

const props = withDefaults(defineProps<{
	mediaType: PlexMediaType;
	libraryId: number;
	detailMode?: boolean;
	mediaDetailItem?: PlexMediaDTO | null;
	allMediaMode?: boolean;
}>(), {
	libraryId: 0,
	detailMode: false,
	allMediaMode: false,
});

defineEmits<{
	(e: 'action', payload: IMediaOverviewBarActions): void;
}>();

const barHeight = ref(85);
const verticalButtonWidth = ref(120);

function isSelected(viewMode: ViewMode) {
	return mediaOverviewStore.getMediaViewMode === viewMode;
}

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
	mediaOverviewStore.clearSort();
	settingsStore.updateDisplayMode(props.mediaType, viewMode);
}
</script>

<style lang="scss">
@use '@/assets/scss/variables' as *;
@use '@/assets/scss/mixins';

.media-overview-bar {
  @extend .default-border;
  min-height: $media-overview-bar-height;
}

.q-fab__label {
  max-height: none;
}
</style>
