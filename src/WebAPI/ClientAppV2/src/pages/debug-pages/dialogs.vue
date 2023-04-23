<template>
	<q-page>
		<q-section>
			<template #header> {{ $t('pages.debug.dialogs.header') }}</template>
			<q-markup-table>
				<q-tr>
					<q-td>
						<q-btn
							color="green"
							outline
							:label="$t('pages.debug.dialogs.buttons.server-dialog')"
							@click="openServerDialog" />
					</q-td>
				</q-tr>
				<q-tr>
					<q-td>
						<q-btn
							color="green"
							outline
							:label="$t('pages.debug.dialogs.buttons.download-confirmation')"
							@click="openDownloadConfirmationDialog" />
					</q-td>
				</q-tr>
				<q-tr>
					<q-td>
						<DebugButton text-id="add-alert" @click="addAlert" />

						<q-btn
							color="green"
							outline
							:label="$t('pages.debug.dialogs.buttons.download-confirmation')"
							@click="openDownloadConfirmationDialog" />
					</q-td>
				</q-tr>
			</q-markup-table>
			<ServerDialog :name="serverDialogName" />
			<DownloadConfirmation :name="downloadConfirmationName" />
		</q-section>
	</q-page>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { useOpenControlDialog } from '@composables/event-bus';
import { DownloadConfirmation } from '#components';
import { AlertService, MediaService } from '@service';
import { PlexMediaDTO } from '@dto/mainApi';

const serverDialogName = 'debugServerDialog';
const downloadConfirmationName = 'debugDownloadConfirmation';

const mediaItem = ref<PlexMediaDTO | null>();
const mediaTableRows = ref<PlexMediaDTO[]>([]);

function openServerDialog(): void {
	useOpenControlDialog(serverDialogName, 1);
}

function openDownloadConfirmationDialog(): void {
	useOpenControlDialog(serverDialogName, 1);
}

function addAlert(): void {
	AlertService.showAlert({ id: 0, title: 'Alert Title', text: 'random alert' });
	AlertService.showAlert({ id: 0, title: 'Alert Title', text: 'random alert' });
}

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
