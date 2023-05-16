<template>
	<!-- Poster display-->
	<q-row id="poster-table" full-height justify="center" data-cy="poster-table">
		<media-poster
			v-for="(item, index) in items"
			:key="item.id"
			:index="index"
			:media-item="item"
			:data-scroll-index="index"
			@download="sendMediaOverviewDownloadCommand($event)" />
	</q-row>
</template>

<script setup lang="ts">
import Log from 'consola';
import { get, set, useScroll } from '@vueuse/core';
import { PlexMediaSlimDTO, PlexMediaType } from '@dto/mainApi';
import {
	listenMediaOverviewScrollToCommand,
	sendMediaOverviewDownloadCommand,
	setMediaOverviewSort,
} from '@composables/event-bus';
import { triggerBoxHighlight } from '@composables/animations';

const autoScrollEnabled = ref(false);
const scrollTargetElement = ref<HTMLElement | null>(null);

const props = withDefaults(
	defineProps<{
		mediaType: PlexMediaType;
		libraryId: number;
		items: PlexMediaSlimDTO[];
		scrollDict?: Record<string, number>;
	}>(),
	{
		scrollDict: { '#': 0 } as any,
	},
);

const posterTableRef = computed(() => document.getElementById('poster-table'));

onMounted(() => {
	// Listen for scroll to letter command
	listenMediaOverviewScrollToCommand((letter) => {
		if (!get(posterTableRef)) {
			Log.error('Could not find container with reference: ', get(posterTableRef));
			return;
		}

		// We have to revert to normal title sort otherwise the index will be wrong
		setMediaOverviewSort({ sort: 'asc', field: 'sortTitle' });
		const index = props.scrollDict[letter];
		// noinspection TypeScriptValidateTypes
		const element: HTMLElement = get(posterTableRef)?.querySelector(`[data-scroll-index="${index}"]`);
		if (!element) {
			Log.error(`Could not find scroll target element for letter ${letter}`, `[data-scroll-index="${index}"]`);
			return;
		}

		set(scrollTargetElement, element);
		set(autoScrollEnabled, true);

		const elementRect = get(scrollTargetElement)?.getBoundingClientRect();
		// Scroll if not visible
		if (elementRect?.bottom >= 0 && (elementRect?.top ?? 0) <= window.innerHeight) {
			triggerBoxHighlight(element);
		} else {
			get(scrollTargetElement)?.scrollIntoView({
				block: 'start',
				behavior: 'smooth',
			});
		}
	});
	// Setup stopped scrolling event listener
	useScroll(get(posterTableRef), {
		onStop() {
			// Don't highlight if the user scrolls manually
			if (!get(autoScrollEnabled)) {
				return;
			}
			set(autoScrollEnabled, false);
			triggerBoxHighlight(get(scrollTargetElement));
		},
	});
});
</script>
<style lang="scss">
@import '@/assets/scss/variables.scss';

#poster-table {
	overflow-y: auto;
	overflow-x: hidden;

	max-height: calc(100vh - $app-bar-height - $media-overview-bar-height);

	&--scroll-container {
		height: 100%;
		width: 100%;
	}
}
</style>
