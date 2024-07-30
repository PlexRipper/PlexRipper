<template>
	<QSection>
		<template #header>
			{{ $t('pages.settings.advanced.database.header') }}
		</template>
		<!--	Reset Database	-->
		<HelpRow
			:label="$t('help.settings.advanced.reset-db.label')"
			:title="$t('help.settings.advanced.reset-db.title')"
			:text="$t('help.settings.advanced.reset-db.text')"
		>
			<WarningButton
				:width="400"
				text-id="reset-db"
				block
				@click="useOpenControlDialog(confirmationDialogName)"
			/>
			<ConfirmationDialog
				text-id="reset-db"
				:name="confirmationDialogName"
				@confirm="resetDatabaseCommand"
			/>
		</HelpRow>
	</QSection>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { settingsApi } from '@api';
import { useOpenControlDialog } from '#imports';

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
