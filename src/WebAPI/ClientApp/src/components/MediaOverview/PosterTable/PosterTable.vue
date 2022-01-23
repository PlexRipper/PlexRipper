<template>
	<v-row no-gutters>
		<!-- Poster display-->
		<v-col>
			<vue-scroll ref="scrollbarposters">
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
			</vue-scroll>
		</v-col>
		<!-- Alphabet Navigation-->
		<alphabet-navigation :items="items" @scroll-to="scrollToIndex" />
	</v-row>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Prop, Ref, Vue } from 'vue-property-decorator';
import { DownloadMediaDTO, PlexMediaType } from '@dto/mainApi';
import ITreeViewItem from '@mediaOverview/MediaTable/types/ITreeViewItem';
import VueScroll from 'vuescroll';

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

	@Ref('scrollbarposters')
	readonly scrollBar!: VueScroll;

	downloadMedia(downloadMediaCommands: DownloadMediaDTO[]): void {
		downloadMediaCommands.forEach((x) => {
			x.plexAccountId = this.activeAccountId;
		});
		this.$emit('download', downloadMediaCommands);
	}

	openDetails(mediaId: number): void {
		this.$emit('open-details', mediaId);
	}

	scrollToIndex(letter: string) {
		if (!this.scrollBar) {
			Log.error('Could not find container with reference: ' + this.scrollBar);
			return;
		}

		let scrollHeight = 0;
		if (letter !== '#') {
			const children = this.scrollBar.$children[0].$children;
			const index = children.findIndex((x) => (x.$el as HTMLElement)?.getAttribute('data-title')?.startsWith(letter)) ?? -1;
			if (index > -1 && children[index]) {
				scrollHeight = (children[index].$el as HTMLElement).offsetTop;
			} else {
				Log.error('Could not find an index with letter ' + letter);
			}
		}

		this.scrollBar.scrollTo({ y: scrollHeight }, 0);
	}
}
</script>
