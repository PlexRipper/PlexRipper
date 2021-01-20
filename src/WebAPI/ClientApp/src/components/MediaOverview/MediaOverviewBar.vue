<template>
	<v-toolbar class="media-overview-bar" outlined :height="barHeight">
		<v-toolbar-title>
			<v-list subheader two-line class="no-background pa-0">
				<v-list-item>
					<v-list-item-avatar v-if="library">
						<v-icon large class="mx-3">{{ library.type | mediaTypeIcon }}</v-icon>
					</v-list-item-avatar>
					<v-list-item-content>
						<v-list-item-title>{{ server ? server.name : '?' }} - {{ library ? library.title : '?' }}</v-list-item-title>
						<v-list-item-subtitle v-if="library">
							{{ mediaCountFormatted }} - <file-size :size="library.mediaSize" />
						</v-list-item-subtitle>
					</v-list-item-content>
				</v-list-item>
			</v-list>
		</v-toolbar-title>

		<v-spacer></v-spacer>
		<!--	Download button	-->
		<vertical-button
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
		<v-menu left bottom offset-y>
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

	@Prop({ required: true, type: Boolean })
	readonly hasSelected!: boolean;

	@Prop({ type: Boolean })
	readonly detailMode!: boolean;

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

	get mediaCountFormatted(): string {
		if (this.library) {
			switch (this.library?.type) {
				case PlexMediaType.Movie:
					return `${this.library.count} Movies`;
				case PlexMediaType.TvShow:
					return `${this.library.count} TvShows - ${this.library.seasonCount} Seasons - ${this.library.episodeCount} Episodes`;
				default:
					return `Library type ${this.library.type} is not supported in the media count`;
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
}
</script>
