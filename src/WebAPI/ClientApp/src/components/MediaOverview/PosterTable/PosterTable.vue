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
import ProgressComponent from '@components/ProgressComponent.vue';
import LoadingSpinner from '@components/LoadingSpinner.vue';
import AlphabetNavigation from '@components/Navigation/AlphabetNavigation.vue';
import ITreeViewItem from '@mediaOverview/MediaTable/types/ITreeViewItem';
import MediaPoster from '@mediaOverview/PosterTable/MediaPoster.vue';

@Component({
	components: {
		LoadingSpinner,
		ProgressComponent,
		AlphabetNavigation,
		MediaPoster,
	},
})
export default class PosterTable extends Vue {
	@Prop({ required: true, type: Array as () => ITreeViewItem[] })
	readonly items!: ITreeViewItem[];

	@Prop({ required: true, type: String })
	readonly mediaType!: PlexMediaType;

	downloadMedia(downloadMediaCommand: DownloadMediaDTO): void {
		this.$emit('download', downloadMediaCommand);
	}

	openDetails(mediaId: number): void {
		this.$emit('open-details', mediaId);
	}
}
</script>
