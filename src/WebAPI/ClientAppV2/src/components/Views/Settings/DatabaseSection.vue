<template>
	<q-section>
		<template #header> {{ $t('pages.settings.advanced.database.header') }}</template>
		<!--	Reset Database	-->
		<q-row>
			<q-col cols="4" align-self="center">
				<help-icon help-id="help.settings.advanced.reset-db" />
			</q-col>
			<!--	Reset Database button	-->
			<q-col cols="8" align-self="center">
				<WarningButton :width="400" text-id="reset-db" block @click="useOpenControlDialog(confirmationDialogName)" />
				<confirmation-dialog text-id="reset-db" :name="confirmationDialogName" @confirm="resetDatabaseCommand" />
			</q-col>
		</q-row>
	</q-section>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { resetDatabase } from '@api/settingsApi';
import { GlobalService } from '@service';
import { useOpenControlDialog } from '#imports';

const router = useRouter();
const confirmationDialogName = 'reset-database-confirmation-dialog';
const resetDatabaseCommand = (): void => {
	useSubscription(
		resetDatabase().subscribe((value) => {
			GlobalService.resetStore();
			if (value.isSuccess) {
				router.push('/setup').then(() => {
					// Refresh the page when we go to the home page to make sure we get all new data.
					location.reload();
				});
			}
		}),
	);
};
</script>
