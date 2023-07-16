<template>
	<Transition>
		<div v-if="showProgressBar">
			<q-linear-progress dark stripe rounded size="20px" :value="getPercentage" color="red" class="q-mt-sm">
				<q-tooltip anchor="bottom middle" self="top middle" :offset="[10, 10]">
					<span>{{ getText }}</span>
				</q-tooltip>
			</q-linear-progress>
		</div>
	</Transition>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { set } from '@vueuse/core';
import { SyncServerProgress } from '@dto/mainApi';
import { useSignalrStore } from '~/store';

const progressList = ref<SyncServerProgress[]>([]);

const getPercentage = computed(() => {
	return sum(progressList.value.map((x) => x.percentage)) / progressList.value.length;
});

const showProgressBar = computed(() => {
	return progressList.value.length > 0 && getPercentage.value <= 100;
});

const getText = computed(() => {
	const finishedCount = progressList.value.filter((x) => x.percentage >= 100).length;
	return `Syncing PlexServer ${finishedCount} of ${progressList.value.length}`;
});

onMounted(() => {
	useSubscription(
		useSignalrStore()
			.getAllSyncServerProgress()
			.subscribe((progress) => {
				set(progressList, progress);
			}),
	);

	// useSubscription(SignalrService.getPlexAccountRefreshProgress(), (data) => {
	// 	if (data) {
	// 		const index = this.accountRefreshProgress.findIndex((x) => x.plexAccountId === data.plexAccountId);
	// 		if (index > -1) {
	// 			if (!data.isComplete) {
	// 				this.accountRefreshProgress.splice(index, 1, data);
	// 			} else {
	// 				this.accountRefreshProgress.splice(index, 1);
	// 			}
	// 		} else {
	// 			this.accountRefreshProgress.push(data);
	// 		}
	// 	}
	// });
});
</script>
