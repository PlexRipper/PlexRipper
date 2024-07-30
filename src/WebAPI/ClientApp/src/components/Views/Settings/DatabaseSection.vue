<template>
	<q-section>
		<template #header>
			{{ t('pages.settings.advanced.database.header') }}
		</template>
		<!--	Reset Database	-->
		<help-row help-id="help.settings.advanced.reset-db">
			<WarningButton
				:width="400"
				text-id="reset-db"
				block
				@click="useOpenControlDialog(confirmationDialogName)"
			/>
			<confirmation-dialog
				text-id="reset-db"
				:name="confirmationDialogName"
				@confirm="resetDatabaseCommand"
			/>
		</help-row>
	</q-section>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { settingsApi } from '@api';
import { useOpenControlDialog } from '#imports';

const { t } = useI18n();

const router = useRouter();
const confirmationDialogName = 'reset-database-confirmation-dialog';
const resetDatabaseCommand = (): void => {
	useSubscription(
		settingsApi.resetDatabaseEndpoint().subscribe((value) => {
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
