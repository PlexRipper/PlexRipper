<template>
	<QPage>
		<template v-if="!loading && mediaItemDetail">
			<MediaDetailsLayout
				:library-id="libraryId"
				:media-item="mediaItemDetail"
				:summary-label="t('components.details-overview.summary')"
				@action="onAction">
				<template #details>
					<tr>
						<td class="media-info-column">
							{{ t('components.music-details.album-count') }}
						</td>
						<td class="media-info-column">
							{{ albumCount }}
						</td>
					</tr>
					<tr>
						<td class="media-info-column">
							{{ t('components.music-details.track-count') }}
						</td>
						<td class="media-info-column">
							{{ trackCount }}
						</td>
					</tr>
					<tr>
						<td class="media-info-column">
							{{ t('components.music-details.duration') }}
						</td>
						<td class="media-info-column">
							<QDuration :value="mediaItemDetail.duration" />
						</td>
					</tr>
					<tr>
						<td class="media-info-column">
							{{ t('components.music-details.size') }}
						</td>
						<td class="media-info-column">
							<QFileSize :size="mediaItemDetail.mediaSize" />
						</td>
					</tr>
					<tr v-if="mediaItemDetail.year > 0">
						<td class="media-info-column">
							{{ t('components.music-details.year') }}
						</td>
						<td class="media-info-column">
							{{ mediaItemDetail.year }}
						</td>
					</tr>
					<tr
						v-for="date in dates"
						:key="date.key">
						<td class="media-info-column">
							{{ date.label }}
						</td>
						<td class="media-info-column">
							<QDateTime
								:text="date.value"
								long-date />
						</td>
					</tr>
				</template>
				<MusicMediaList :media-item="mediaItemDetail" />
			</MediaDetailsLayout>
		</template>

		<div
			v-else-if="errorMessage"
			class="q-pa-lg text-center"
			data-cy="music-details-error">
			<QAlert type="error">
				{{ errorMessage }}
			</QAlert>
			<q-btn
				v-if="validRoute"
				class="q-mt-md"
				:label="t('components.media-overview.retry-media-load')"
				@click="loadArtist" />
			<q-btn
				flat
				class="q-mt-md"
				:label="t('general.commands.back')"
				@click="backToArtists" />
		</div>

		<MediaComparisonDetailsDialog />
		<DownloadConfirmation @download="downloadStore.downloadMedia($event)" />
		<QLoadingOverlay :loading="loading" />
	</QPage>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import { Subject, of, switchMap, catchError, take } from 'rxjs';
import { useSubscription } from '@vueuse/rxjs';
import type { PlexMediaDTO } from '@dto';
import { PlexMediaType } from '@dto';
import type { IMediaOverviewBarActions } from '@interfaces';
import { useDialogStore, useDownloadStore, useMediaOverviewStore, useMediaStore, useSettingsStore } from '@store';
import { listenMediaOverviewDownloadCommand } from '@composables/event-bus';

const route = useRoute();
const router = useRouter();
const { t } = useI18n();
const mediaStore = useMediaStore();
const mediaOverviewStore = useMediaOverviewStore();
const dialogStore = useDialogStore();
const downloadStore = useDownloadStore();
const settingsStore = useSettingsStore();
const loading = ref(true);
const errorMessage = ref('');
const mediaItemDetail = ref<PlexMediaDTO | null>(null);
const requests = new Subject<void>();
const libraryId = computed(() => +(route.params.id as string));
const artistId = computed(() => +(route.params.artistId as string));
const validRoute = computed(() => [get(libraryId), get(artistId)].every((id) => Number.isSafeInteger(id) && id > 0));
const albumCount = computed(() => get(mediaItemDetail)?.children.length ?? 0);
const trackCount = computed(() => get(mediaItemDetail)?.children.reduce((count, album) => count + album.children.length, 0) ?? 0);
const dates = computed(() => [
	{ key: 'originallyAvailableAt', label: t('components.music-details.originally-available'), value: get(mediaItemDetail)?.originallyAvailableAt },
	{ key: 'addedAt', label: t('components.music-details.added-at'), value: get(mediaItemDetail)?.addedAt },
	{ key: 'updatedAt', label: t('components.music-details.updated-at'), value: get(mediaItemDetail)?.updatedAt },
].filter((date): date is { key: string; label: string; value: string } => !!date.value && Number.isFinite(Date.parse(date.value)) && !date.value.startsWith('0001-')));

function backToArtists(): void {
	router.push(get(libraryId) > 0 ? `/music/${get(libraryId)}` : '/music');
}

function onAction(event: IMediaOverviewBarActions): void {
	if (event === 'back')
		backToArtists();
}

function loadArtist(): void {
	set(loading, get(validRoute));
	set(errorMessage, get(validRoute) ? '' : t('components.music-details.invalid-route'));
	set(mediaItemDetail, null);
	mediaOverviewStore.$patch({
		libraryId: get(libraryId),
		downloadButtonVisible: false,
		isDetailView: true,
		selection: { keys: [], allSelected: false, indexKey: get(libraryId) },
	});
	requests.next();
}

useSubscription(requests.pipe(
	switchMap(() => get(validRoute)
		? mediaStore.getMediaDataDetailById(get(artistId), PlexMediaType.MusicArtist).pipe(take(1), catchError(() => of(null)))
		: of(null)),
).subscribe((mediaDetail) => {
	const validItem = mediaDetail?.type === PlexMediaType.MusicArtist
		&& mediaDetail.id === get(artistId)
		&& mediaDetail.plexLibraryId === get(libraryId);
	set(mediaItemDetail, validItem ? mediaDetail : null);
	if (validItem)
		mediaOverviewStore.lastMediaItemViewed = mediaDetail;
	else if (get(validRoute))
		set(errorMessage, t('components.music-details.load-error'));
	set(loading, false);
}));

listenMediaOverviewDownloadCommand((command) => {
	const first = command[0];
	const current = get(mediaItemDetail);
	if (!current || get(loading) || command.some((item) => item.plexLibraryId !== current.plexLibraryId
		|| item.plexServerId !== current.plexServerId
		|| ![PlexMediaType.MusicArtist, PlexMediaType.MusicAlbum, PlexMediaType.MusicTrack].includes(item.type)))
		return;
	if (!first) return;
	if (settingsStore.isConfirmationEnabled(first.type)) {
		dialogStore.openMediaConfirmationDownloadDialog(command);
	} else {
		downloadStore.downloadMedia({
			downloadMedias: command,
			customDestinationFolderPath: '',
			destinationFolderPathId: null,
		});
	}
});

watch([libraryId, artistId], loadArtist, { immediate: true, flush: 'sync' });

onBeforeUnmount(() => {
	requests.complete();
	set(mediaItemDetail, null);
	mediaOverviewStore.$patch({ downloadButtonVisible: false, isDetailView: false });
});

definePageMeta({ scrollToTop: false });
</script>
