<template>
	<!--	Refresh Library Screen	-->
	<QRow
		align="start"
		class="q-pt-xl"
		cy="refresh-library-container"
		full-height>
		<QCol
			text-align="center">
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
import { useI18n, useLibraryStore, useServerStore } from '#imports';
import { get, set } from '@vueuse/core';

const { t } = useI18n();
const libraryStore = useLibraryStore();
const serverStore = useServerStore();

const props = defineProps<{
	libraryId: number;
}>();

const cancelling = ref(false);

const library = computed(() => libraryStore.getLibrary(props.libraryId));
const libraryProgress = computed(() => libraryStore.getLibraryProgress(props.libraryId));

const refreshingText = computed(() => {
	const server = libraryStore.getServerByLibraryId(props.libraryId);
	return t('components.media-overview.is-refreshing', {
		library: get(library) ? libraryStore.getLibraryName(props.libraryId) : t('general.commands.unknown'),
		server: server ? serverStore.getServerName(server.id) : t('general.commands.unknown'),
	});
});

function cancelSync() {
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
