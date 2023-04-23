<template>
	<q-page>
		<q-row v-if="firstTimeSetup">
			<q-col cols="12">
				<h2>{{ $t('pages.home.setup-question') }}</h2>
				<q-row justify="center">
					<q-col cols="3">
						<NavigationSkipSetupButton block @click="skipSetup()" />
					</q-col>
					<q-col cols="3">
						<GoToButton text-id="go-to-setup-page" block to="/setup" color="green" />
					</q-col>
				</q-row>
			</q-col>
		</q-row>
		<q-row v-else>
			<q-col>
				<h1>{{ $t('pages.home.header') }}</h1>
			</q-col>
		</q-row>
	</q-page>
</template>

<script setup lang="ts">
import Log from 'consola';
import { ref, onMounted } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { set } from '@vueuse/core';
import { SettingsService } from '@service';

const firstTimeSetup = ref(false);

const skipSetup = () => {
	useSubscription(
		SettingsService.updateGeneralSettings('firstTimeSetup', false).subscribe(() => {
			Log.info('Setup process skipped');
		}),
	);
};

onMounted(() => {
	useSubscription(
		SettingsService.getFirstTimeSetup().subscribe((state) => {
			set(firstTimeSetup, state);
		}),
	);
});
</script>
