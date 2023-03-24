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
				<WarningButton :width="400" text-id="reset-db" @click="confirmationDialog = true" />
				<confirmation-dialog
					text-id="reset-db"
					:dialog="confirmationDialog"
					@confirm="resetDatabaseCommand"
					@cancel="confirmationDialog = false" />
			</q-col>
		</q-row>
	</q-section>
</template>

<script setup lang="ts">
import Log from 'consola';
import { ref } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { resetDatabase } from '@api/settingsApi';
import { GlobalService } from '@service';

const confirmationDialog = ref(false);

const router = useRouter();

const resetDatabaseCommand = (): void => {
	useSubscription(
		resetDatabase().subscribe((value) => {
			GlobalService.resetStore();
			Log.debug('reset db', value);
			if (value.isSuccess) {
				router.push('/setup');
			}
		}),
	);
};
</script>
