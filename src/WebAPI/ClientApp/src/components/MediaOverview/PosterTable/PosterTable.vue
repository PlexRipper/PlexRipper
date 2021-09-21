<template>
	<v-row no-gutters>
		<!-- Poster display-->
		<v-col>
			<perfect-scrollbar ref="scrollbarposters">
				<v-row class="poster-overview" justify="center">
					<template v-for="item in items">
						<media-poster
							:key="item.id"
							:media-item="item"
							:media-type="mediaType"
							@download="downloadMedia"
							@open-details="openDetails"
						/>
					</template>
				</v-row>
			</perfect-scrollbar>
		</v-col>
		<!-- Alphabet Navigation-->
		<alphabet-navigation :items="items" container-ref="scrollbarposters" />
	</v-row>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { DownloadMediaDTO, PlexMediaType } from '@dto/mainApi';
import ITreeViewItem from '@mediaOverview/MediaTable/types/ITreeViewItem';

@Component
export default class PosterTable extends Vue {
	@Prop({ required: true, type: Array as () => ITreeViewItem[] })
	readonly items!: ITreeViewItem[];

	@Prop({ required: true, type: String })
	readonly mediaType!: PlexMediaType;

	@Prop({ required: false, type: Number })
	readonly libraryId!: number;

	@Prop({ required: true, type: Number })
	readonly activeAccountId!: number;

	downloadMedia(downloadMediaCommands: DownloadMediaDTO[]): void {
		downloadMediaCommands.forEach((x) => {
			x.libraryId = this.libraryId;
			x.plexAccountId = this.activeAccountId;
		});
		this.$emit('download', downloadMediaCommands);
	}

	openDetails(mediaId: number): void {
		this.$emit('open-details', mediaId);
	}
}
</script>
