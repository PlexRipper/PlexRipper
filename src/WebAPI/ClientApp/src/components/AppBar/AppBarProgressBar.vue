<template>
	<QCol
		v-if="showProgressBar"
		cols="grow">
		<q-linear-progress
			:value="getPercentage / 100"
			class="q-mt-sm"
			color="red"
			dark
			rounded
			size="20px"
			stripe>
			<q-tooltip
				:offset="[10, 10]"
				anchor="bottom middle"
				self="top middle">
				<span>{{ getText }}</span>
			</q-tooltip>
		</q-linear-progress>
	</QCol>
</template>

<script lang="ts" setup>
import { useSubscription } from '@vueuse/rxjs';
import { get, set, watchDebounced } from '@vueuse/core';
import { JobStatus, type SyncServerMediaProgress } from '@dto';
import { useBackgroundJobsStore, useSignalrStore } from '@store';

const { t } = useI18n();
const signalRStore = useSignalrStore();
const backgroundJobsStore = useBackgroundJobsStore();

const showProgressBar = ref(false);
const progressList = ref<SyncServerMediaProgress[]>([]);

const getPercentage = computed(() => {
	return sum(get(progressList).map((x) => x.percentage)) / get(progressList).length;
});

const getText = computed(() => {
	const finishedCount = progressList.value.filter((x) => x.percentage >= 100).length;
	return t('components.app-bar-progress-bar.tooltip-text', {
		finishedCount,
		totalCount: get(progressList).flatMap((x) => x.libraryProgresses).length,
	});
});

watchDebounced(getPercentage, (value) => {
	if (value === 100) {
		set(progressList, []);
	}
}, { debounce: 500, maxWait: 10000 });

onMounted(() => {
	useSubscription(
		signalRStore
			.getAllSyncServerMediaProgress()
			.subscribe((progress) => {
				set(showProgressBar, true);
				set(progressList, progress);
			}),
	);

	useSubscription(
		backgroundJobsStore.getSyncServerMediaJobUpdate(JobStatus.Started)
			.subscribe(() => {
				set(showProgressBar, true);
			}),
	);

	useSubscription(
		backgroundJobsStore.getSyncServerMediaJobUpdate(JobStatus.Completed)
			.subscribe(() => {
				setTimeout(() => {
					set(showProgressBar, false);
				}, 2000);
			}),
	);
});
</script>
