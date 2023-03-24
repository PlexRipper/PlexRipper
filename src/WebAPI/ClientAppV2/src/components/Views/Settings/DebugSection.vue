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
	</q-section>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { resetDatabase } from '@api/settingsApi';
import { AlertService } from '@service';

const router = useRouter();

const addAlert = (): void => {
	AlertService.showAlert({ id: 0, title: 'Alert Title', text: 'random alert' });
	AlertService.showAlert({ id: 0, title: 'Alert Title', text: 'random alert' });
};

// TODO Fix the reset button for the database
const resetDatabaseCommand = (): void => {
	useSubscription(
		resetDatabase().subscribe(() => {
			router.push('/setup');
		}),
	);
};
</script>
