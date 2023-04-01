<template>
	<q-section>
		<template #header> {{ $t('pages.settings.debug.header') }}</template>
		<!--	Reset Database	-->
		<q-row>
			<q-col cols="4" align-self="center">
				<DebugButton text-id="add-alert" @click="addAlert" />
			</q-col>
			<q-col cols="8" align-self="center" />
		</q-row>
		<div :style="{ width: '100px', height: '100px', backgroundColor: color }"></div>
		<q-row>
			<q-col style="height: 1000px">
				<q-scroll-area class="fit">
					<DownloadConfirmation ref="downloadConfirmationRef" />
				</q-scroll-area>
			</q-col>
		</q-row>
	</q-section>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { resetDatabase } from '@api/settingsApi';
import { AlertService, MediaService } from '@service';
import { getMediaTableColumns } from '~/composables/mediaTableColumns';
import { PlexMediaDTO } from '@dto/mainApi';
import { DownloadConfirmation } from '#components';

const router = useRouter();
const mediaItem = ref<PlexMediaDTO | null>();
const mediaTableColumns = getMediaTableColumns();
const mediaTableRows = ref<PlexMediaDTO[]>([]);

const color = ref('red');
const addAlert = (): void => {
	AlertService.showAlert({ id: 0, title: 'Alert Title', text: 'random alert' });
	AlertService.showAlert({ id: 0, title: 'Alert Title', text: 'random alert' });
};

const downloadConfirmationRef = ref<InstanceType<typeof DownloadConfirmation> | null>(null);

// TODO Fix the reset button for the database
const resetDatabaseCommand = (): void => {
	useSubscription(
		resetDatabase().subscribe(() => {
			router.push('/setup');
		}),
	);
};

const openDownloadConfirmation = (): void => {
	const testData = [
		{
			mediaIds: [25],
			type: 'TvShow',
			plexServerId: 1,
			plexLibraryId: 9,
		},
	];
	downloadConfirmationRef.value.openDialog(testData);
};

onMounted(() => {
	openDownloadConfirmation();
	useSubscription(
		MediaService.getTvShowMediaData(32).subscribe((data) => {
			if (data) {
				mediaTableRows.value = [data];
				mediaItem.value = data;
			}
		}),
	);
});
</script>
