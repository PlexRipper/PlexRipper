<template>
	<!-- Poster display-->
	<q-row id="poster-table" justify="center" data-cy="poster-table">
		<media-poster
			v-for="(item, index) in items"
			:key="item.id"
			:index="index"
			:media-item="item"
			:media-type="mediaType"
			:data-scroll-index="index"
			@download="downloadMedia($event)"
			@open-details="$emit('open-details', $event)" />
	</q-row>
</template>

<script setup lang="ts">
import Log from 'consola';
import { defineProps, defineEmits, ref, withDefaults, onMounted } from 'vue';
import { get, set, useScroll } from '@vueuse/core';
import { DownloadMediaDTO, PlexMediaDTO, PlexMediaSlimDTO, PlexMediaType } from '@dto/mainApi';
import { listenMediaOverviewScrollToCommand, setMediaOverviewSort } from '@composables/event-bus';
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

const emit = defineEmits<{
	(e: 'open-details', mediaId: number): void;
	(e: 'download', downloadMediaCommands: DownloadMediaDTO[]): void;
}>();

const posterTableRef = computed(() => document.getElementById('poster-table'));

const downloadMedia = (downloadMediaCommands: DownloadMediaDTO[]): void => {
	downloadMediaCommands.forEach((x) => {
		// x.plexAccountId = props.activeAccountId ?? 0;
	});
	emit('download', downloadMediaCommands);
};

onMounted(() => {
	(function setGlowEffectRx() {
		const glowEffects = document.querySelectorAll('.highlight-border-box');

		glowEffects.forEach((glowEffect) => {
			const glowLines = glowEffect.querySelectorAll('rect');
			const rx = getComputedStyle(glowEffect).borderRadius;

			glowLines.forEach((line) => {
				line.setAttribute('rx', rx);
			});
		});
	})();

	// Listen for scroll to letter command
	listenMediaOverviewScrollToCommand((letter) => {
		if (!get(posterTableRef)) {
			Log.error('Could not find container with reference: ', get(posterTableRef));
			return;
		}
		Log.debug('posterTableRef', get(posterTableRef));
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
		if (elementRect?.bottom >= 0 && elementRect?.top <= window.innerHeight) {
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
//.poster-overview,
//.alphabet-navigation {
//	height: calc(100vh - 85px - 48px);
//	width: 100%;
//	// 8% reserved for the media-overview-bar
//	// height: 92%;
//}
</style>
