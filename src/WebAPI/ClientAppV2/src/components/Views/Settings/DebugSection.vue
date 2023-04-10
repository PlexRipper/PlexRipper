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
		<q-row>
			<q-col style="height: 1000px">
				<q-scroll-area class="fit">
					<DownloadsTable :download-rows="downloadRows.downloads" :server-id="downloadRows.id" />
					<DownloadConfirmation />
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
import { generateDownloadTasks } from '@mock/mock-download-task';

const router = useRouter();
const mediaItem = ref<PlexMediaDTO | null>();
const mediaTableColumns = getMediaTableColumns();
const mediaTableRows = ref<PlexMediaDTO[]>([]);
const downloadRows = generateDownloadTasks(1, { tvShowDownloadTask: 10 });

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

onMounted(() => {
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
