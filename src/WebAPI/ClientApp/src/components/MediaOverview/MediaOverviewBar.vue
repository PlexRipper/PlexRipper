<template>
	<v-toolbar class="media-overview-bar" outlined :height="barHeight">
		<!--	Title	-->
		<v-toolbar-title>
			<v-row justify="center" no-gutters>
				<v-col v-if="detailMode" cols="auto">
					<v-list two-line class="no-background">
						<v-list-item>
							<p-btn icon-mode icon="mdi-arrow-left" x-large @click="back" />
						</v-list-item>
					</v-list>
				</v-col>
				<v-col cols="auto">
					<v-list subheader two-line class="no-background pa-0">
						<v-list-item>
							<v-list-item-avatar v-if="library">
								<media-type-icon class="mx-3" :size="36" :media-type="library.type" />
							</v-list-item-avatar>
							<v-list-item-content>
								<v-list-item-title>{{ server ? server.name : '?' }} - {{ library ? library.title : '?' }}</v-list-item-title>
								<v-list-item-subtitle v-if="library">
									{{ detailMode ? mediaCountFormatted : libraryCountFormatted }} -
									<file-size :size="mediaSize" />
								</v-list-item-subtitle>
							</v-list-item-content>
						</v-list-item>
					</v-list>
				</v-col>
			</v-row>
		</v-toolbar-title>

		<v-spacer></v-spacer>
		<!--	Download button	-->
		<vertical-button
			v-if="!hideDownloadButton"
			icon="mdi-download"
			label="Download"
			:height="barHeight"
			:width="verticalButtonWidth"
			:disabled="!hasSelected"
			@click="download"
		/>

		<!--	Refresh library button	-->
		<vertical-button
			v-if="!detailMode"
			icon="mdi-refresh"
			label="Refresh"
			:height="barHeight"
			:width="verticalButtonWidth"
			@click="refreshLibrary"
		/>

		<!--	View mode	-->
		<v-menu v-if="!detailMode" left bottom offset-y>
			<template #activator="{ on, attrs }">
				<vertical-button v-bind="attrs" icon="mdi-eye" label="View" :height="barHeight" :width="verticalButtonWidth" v-on="on" />
			</template>
			<!-- View mode options -->
			<v-list>
				<template v-for="(viewOption, i) in viewOptions">
					<v-list-item :key="i" @click="changeView(viewOption.viewMode)">
						<v-list-item-content>
							<v-list-item-title>{{ viewOption.label }}</v-list-item-title>
						</v-list-item-content>
						<!--	Is selected icon	-->
						<v-list-item-icon>
							<v-icon v-if="isSelected(viewOption.viewMode)">mdi-check</v-icon>
						</v-list-item-icon>
					</v-list-item>
				</template>
			</v-list>
		</v-menu>
	</v-toolbar>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import type { PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import { PlexMediaType, ViewMode } from '@dto/mainApi';
import VerticalButton from '@components/General/VerticalButton.vue';
import ITreeViewItem from '@mediaOverview/MediaTable/types/ITreeViewItem';
import _ from 'lodash';

interface IViewOptions {
	label: string;
	viewMode: ViewMode;
}

@Component({
	components: { VerticalButton },
})
export default class MediaOverviewBar extends Vue {
	@Prop({ required: false, type: Object as () => PlexServerDTO | null })
	readonly server!: PlexServerDTO | null;

	@Prop({ required: false, type: Object as () => PlexLibraryDTO | null })
	readonly library!: PlexLibraryDTO | null;

	@Prop({ required: false, type: String })
	readonly viewMode!: ViewMode;

	@Prop({ type: Object as () => ITreeViewItem })
	readonly mediaItem!: ITreeViewItem | null;

	@Prop({ required: true, type: Boolean })
	readonly hasSelected!: boolean;

	@Prop({ type: Boolean })
	readonly detailMode!: boolean;

	@Prop({ type: Boolean, default: false })
	readonly hideDownloadButton!: boolean;

	readonly barHeight: number = 85;
	readonly verticalButtonWidth: number = 120;

	refreshLibrary(): void {
		this.$emit('refresh-library', this.library?.id);
	}

	download(): void {
		this.$emit('download');
	}

	changeView(viewMode: ViewMode): void {
		this.$emit('view-change', viewMode);
	}

	isSelected(viewMode: ViewMode): boolean {
		return this.viewMode === viewMode;
	}

	get mediaSize(): number {
		return this.detailMode ? this.mediaItem?.mediaSize ?? 0 : this.library?.mediaSize ?? 0;
	}

	get libraryCountFormatted(): string {
		if (this.library) {
			switch (this.library?.type) {
				case PlexMediaType.Movie:
					return `${this.library.count} Movies`;
				case PlexMediaType.TvShow:
					return `${this.library.count} TvShows - ${this.library.seasonCount} Seasons - ${this.library.episodeCount} Episodes`;
				default:
					return `Library type ${this.library?.type} is not supported in the media count`;
			}
		}
		return 'unknown media count';
	}

	get mediaCountFormatted(): string {
		if (this.mediaItem) {
			switch (this.library?.type) {
				case PlexMediaType.Movie:
					return `1 Movie`;
				case PlexMediaType.TvShow:
					return `1 TvShow - ${this.mediaItem.children?.length} Seasons - ${_.sum(
						this.mediaItem.children?.map((x) => x.childCount),
					)} Episodes`;
				default:
					return `Library type ${this.library?.type} is not supported in the media count`;
			}
		}
		return 'unknown media count';
	}

	get viewOptions(): IViewOptions[] {
		return [
			{
				label: 'Poster View',
				viewMode: ViewMode.Poster,
			},
			{
				label: 'Table View',
				viewMode: ViewMode.Table,
			},
			// {
			// 	label: 'Overview',
			// 	viewMode: ViewMode.Overview,
			// },
		];
	}

	back(): void {
		this.$emit('back');
	}
}
</script>
