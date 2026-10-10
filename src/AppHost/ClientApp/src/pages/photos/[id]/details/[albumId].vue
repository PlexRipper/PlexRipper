<template>
	<QPage data-cy="photo-album-details">
		<template v-if="mediaItem && !loading && !error">
			<MediaDetailsLayout
				:library-id="libraryId"
				:media-item="mediaItem"
				:summary-label="t('components.details-overview.summary')"
				@action="onAction">
				<template #details>
					<tr>
						<td class="media-info-column">
							{{ t('components.photo-details.asset-count') }}
						</td>
						<td class="media-info-column">
							{{ mediaItem.childCount }}
						</td>
					</tr>
					<tr>
						<td class="media-info-column">
							{{ t('components.photo-details.size') }}
						</td>
						<td class="media-info-column">
							<QFileSize :size="mediaItem.mediaSize" />
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
								short-date />
						</td>
					</tr>
				</template>
				<PhotoMediaList
					:key="`${libraryId}-${mediaItem.id}`"
					:media-item="mediaItem" />
			</MediaDetailsLayout>
		</template>
		<p
			v-else-if="loading"
			role="status"
			class="q-pa-md"
			data-cy="photo-details-loading">
			{{ t('components.photo-details.loading') }}
		</p>
		<div
			v-else
			class="q-pa-md">
			<p
				role="alert"
				data-cy="photo-details-error">
				{{ validRoute ? t('components.photo-details.load-error') : t('components.photo-details.invalid-route') }}
			</p>
			<q-btn
				v-if="validRoute"
				class="photo-details__button"
				:label="t('components.photo-details.retry')"
				@click="requestDetail" />
			<q-btn
				flat
				class="photo-details__button"
				:label="t('general.commands.back')"
				@click="backToAlbums" />
		</div>
		<DownloadConfirmation @download="downloadStore.downloadMedia($event)" />
	</QPage>
</template>

<script setup lang="ts">
import { Subject, of, switchMap, catchError, take } from 'rxjs';
import { get, set } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import { PlexMediaType, type PlexMediaDTO } from '@dto';
import type { IMediaOverviewBarActions } from '@interfaces';
import { useDialogStore, useDownloadStore, useMediaOverviewStore, useMediaStore, useSettingsStore } from '@store';
import { listenMediaOverviewDownloadCommand } from '@composables/event-bus';

definePageMeta({ scrollToTop: false });
const route = useRoute();
const router = useRouter();
const { t } = useI18n();
const mediaStore = useMediaStore();
const mediaOverviewStore = useMediaOverviewStore();
const settingsStore = useSettingsStore();
const dialogStore = useDialogStore();
const downloadStore = useDownloadStore();
const libraryId = computed(() => Number(route.params.id));
const albumId = computed(() => Number(route.params.albumId));
const validRoute = computed(() => [get(libraryId), get(albumId)].every((id) => Number.isSafeInteger(id) && id > 0));
const mediaItem = ref<PlexMediaDTO | null>(null);
const loading = ref(false);
const error = ref(false);
const requests = new Subject<void>();
const dates = computed(() => [
	{ key: 'originallyAvailableAt', label: t('components.photo-details.originally-available'), value: get(mediaItem)?.originallyAvailableAt },
	{ key: 'addedAt', label: t('components.photo-details.added-at'), value: get(mediaItem)?.addedAt },
	{ key: 'updatedAt', label: t('components.photo-details.updated-at'), value: get(mediaItem)?.updatedAt },
].filter((date): date is { key: string; label: string; value: string } => !!date.value && Number.isFinite(Date.parse(date.value)) && !date.value.startsWith('0001-')));

function backToAlbums() {
	router.push(get(libraryId) > 0 ? `/photos/${get(libraryId)}` : '/photos');
}
function onAction(action: IMediaOverviewBarActions) {
	if (action === 'back') {
		backToAlbums();
	}
}
function requestDetail() {
	set(mediaItem, null);
	set(error, false);
	set(loading, get(validRoute));
	mediaOverviewStore.$patch({
		libraryId: get(libraryId),
		isDetailView: true,
		downloadButtonVisible: false,
		selection: { keys: [], allSelected: false, indexKey: get(libraryId) },
	});
	requests.next();
}

useSubscription(requests.pipe(
	switchMap(() => get(validRoute)
		? mediaStore.getMediaDataDetailById(get(albumId), PlexMediaType.PhotoAlbum).pipe(take(1), catchError(() => of(null)))
		: of(null)),
).subscribe((item) => {
	const validItem = item?.type === PlexMediaType.PhotoAlbum && item.id === get(albumId) && item.plexLibraryId === get(libraryId);
	set(mediaItem, validItem ? item : null);
	set(error, !validItem);
	set(loading, false);
	if (validItem) {
		mediaOverviewStore.lastMediaItemViewed = item;
	}
}));
watch([libraryId, albumId], requestDetail, { immediate: true, flush: 'sync' });

listenMediaOverviewDownloadCommand((command) => {
	const current = get(mediaItem);
	if (!current || get(loading) || command.length === 0
		|| command.some((item) => item.plexLibraryId !== current.plexLibraryId
			|| item.plexServerId !== current.plexServerId
			|| ![PlexMediaType.PhotoAlbum, PlexMediaType.PhotoImage].includes(item.type))) {
		return;
	}
	if (settingsStore.isConfirmationEnabled(command[0]!.type)) {
		dialogStore.openMediaConfirmationDownloadDialog(command);
	} else {
		downloadStore.downloadMedia({ downloadMedias: command, customDestinationFolderPath: '', destinationFolderPathId: null });
	}
});
onBeforeUnmount(() => {
	requests.complete();
	mediaOverviewStore.$patch({ isDetailView: false, downloadButtonVisible: false });
});
</script>

<style scoped lang="scss">
.photo-details__button {
	min-width: 44px;
	min-height: 44px;
}
</style>
