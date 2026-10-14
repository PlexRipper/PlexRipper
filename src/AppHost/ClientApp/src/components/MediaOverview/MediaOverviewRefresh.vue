<template>
	<template v-if="props.mode === 'queued'">
		<QBanner
			v-if="props.variant === 'banner'"
			class="media-overview-library-sync-queued-banner"
			dense
			role="status"
			aria-live="polite"
			data-cy="media-overview-library-sync-queued-banner">
			<template #avatar>
				<q-icon
					name="mdi-clock-outline"
					aria-hidden="true" />
			</template>
			<div
				class="text-weight-medium"
				data-cy="media-overview-library-sync-queued-title">
				{{ t('components.media-overview.sync-queued-title') }}
			</div>
			<div data-cy="media-overview-library-sync-queued-message">
				{{ queuedMessage }}
			</div>
			<div class="q-mt-sm">
				{{ t('components.media-overview.sync-queued-automatic') }}
			</div>
			<div class="row q-gutter-sm q-mt-sm">
				<BaseButton
					flat
					cy="view-library-sync-activity-button"
					:label="t('components.media-overview.view-sync-activity')"
					@click="openActivity" />
				<BaseButton
					outline
					cy="cancel-queued-library-sync-button"
					:label="t('components.media-overview.cancel-queued-refresh')"
					:loading="cancelling"
					@click="cancelSync" />
			</div>
		</QBanner>
		<QRow
			v-else
			align="start"
			class="q-pt-xl"
			cy="refresh-library-container"
			full-height>
			<QCol text-align="center">
				<QBanner
					class="q-mx-md"
					role="status"
					aria-live="polite"
					data-cy="media-overview-library-sync-queued-panel">
					<template #avatar>
						<q-icon
							name="mdi-clock-outline"
							aria-hidden="true" />
					</template>
					<div
						class="text-h6"
						data-cy="media-overview-library-sync-queued-title">
						{{ t('components.media-overview.sync-queued-title') }}
					</div>
					<div data-cy="media-overview-library-sync-queued-message">
						{{ queuedMessage }}
					</div>
					<div class="q-mt-sm">
						{{ t('components.media-overview.sync-queued-automatic') }}
					</div>
					<div class="row justify-center q-gutter-sm q-mt-md">
						<BaseButton
							flat
							cy="view-library-sync-activity-button"
							:label="t('components.media-overview.view-sync-activity')"
							@click="openActivity" />
						<BaseButton
							outline
							cy="cancel-queued-library-sync-button"
							:label="t('components.media-overview.cancel-queued-refresh')"
							:loading="cancelling"
							@click="cancelSync" />
					</div>
				</QBanner>
			</QCol>
		</QRow>
	</template>
	<QRow
		v-else
		align="start"
		class="q-pt-xl"
		cy="refresh-library-container"
		full-height>
		<QCol text-align="center">
			<ProgressComponent
				:percentage="libraryProgress?.percentage ?? -1"
				:text="refreshingText"
				circular-mode
				:indeterminate="libraryProgress?.percentage == 0"
				class="q-my-lg" />
			<div>
				<QCountdown
					class="q-my-md"
					:value="libraryProgress?.timeRemaining ?? ''" />
				<QRow justify="around">
					<QCol cols="6">
						<QRow
							v-for="item in libraryProgress?.items"
							:key="item.mediaType"
							gutter="sm"
							justify="between"
							class="library-media-sync-progress-row q-my-sm">
							<QCol cols="auto">
								<QMediaTypeIcon
									class="q-pr-sm"
									:media-type="item.mediaType"
									:size="20" />
							</QCol>
							<QCol>
								<QProgressBar
									:value="item.percentage"
									:cy="`library-media-sync-progress-row-${item.mediaType}-progress-bar`" />
							</QCol>
							<QCol cols="1">
								<QText
									v-if="item.received > 0 && item.total > 0"
									:value="`${item.received}/${item.total}`"
									align="right"
									:cy="`library-media-sync-progress-row-${item.mediaType}-count`" />
							</QCol>
						</QRow>
						<QRow
							v-if="(libraryProgress?.percentage ?? 0) >= 100"
							justify="around">
							<QCol cols="auto">
								<QText value="Updating database with all new data, please wait" />
							</QCol>
						</QRow>
					</QCol>
				</QRow>
				<QRow
					justify="around"
					class="q-mt-lg">
					<QCol cols="auto">
						<CancelButton
							cy="cancel-library-sync-button"
							:loading="cancelling"
							@click="cancelSync" />
					</QCol>
				</QRow>
			</div>
		</QCol>
	</QRow>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import { useDialogStore, useLibraryStore, useServerStore } from '@store';
import { useI18n } from '#imports';

const { t } = useI18n();
const libraryStore = useLibraryStore();
const serverStore = useServerStore();
const dialogStore = useDialogStore();

const props = withDefaults(defineProps<{
	libraryId: number;
	mode?: 'processing' | 'queued';
	variant?: 'panel' | 'banner';
}>(), {
	mode: 'processing',
	variant: 'panel',
});

const cancelling = ref(false);

const library = computed(() => libraryStore.getLibrary(props.libraryId));
const libraryProgress = computed(() => libraryStore.getLibraryProgress(props.libraryId));
const queue = computed(() => libraryStore.getLibrarySyncQueue(props.libraryId));

const refreshingText = computed(() => {
	const server = libraryStore.getServerByLibraryId(props.libraryId);
	return t('components.media-overview.is-refreshing', {
		library: get(library) ? libraryStore.getLibraryName(props.libraryId) : t('general.commands.unknown'),
		server: server ? serverStore.getServerName(server.id) : t('general.commands.unknown'),
	});
});

const queuedMessage = computed(() => {
	const currentQueue = get(queue);
	const reason = libraryStore.getLibrarySyncWaitingReason(props.libraryId);
	if (reason === 'offline') {
		return t('components.media-overview.sync-queued-offline');
	}

	if (reason === 'busy' && currentQueue) {
		const processing = libraryStore.getProcessingLibrarySyncQueue(currentQueue.plexServerId);
		if (processing) {
			return t('components.media-overview.sync-queued-busy', {
				library: libraryStore.getLibraryName(processing.plexLibraryId),
				server: serverStore.getServerName(currentQueue.plexServerId),
			});
		}
	}

	return t('components.media-overview.sync-queued-waiting');
});

function openActivity(): void {
	const currentQueue = get(queue);
	if (currentQueue) {
		dialogStore.openSyncServerMediaDialog(currentQueue.plexServerId);
		return;
	}

	dialogStore.openSyncServerMediaDialog();
}

function cancelSync(): void {
	if (get(cancelling)) {
		return;
	}

	set(cancelling, true);
	useSubscription(
		libraryStore.cancelLibrarySync(props.libraryId).subscribe({
			complete: () => {
				set(cancelling, false);
			},
			error: () => {
				set(cancelling, false);
			},
		}),
	);
}
</script>
