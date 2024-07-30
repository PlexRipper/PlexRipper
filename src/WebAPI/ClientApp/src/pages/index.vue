<template>
	<q-page>
		<q-row v-if="settingsStore.generalSettings.firstTimeSetup">
			<q-col cols="12">
				<h2>{{ t('pages.home.setup-question') }}</h2>
				<q-row justify="center">
					<q-col cols="3">
						<NavigationSkipSetupButton
							block
							@click="skipSetup()"
						/>
					</q-col>
					<q-col cols="3">
						<GoToButton
							text-id="go-to-setup-page"
							block
							to="/setup"
							color="positive"
						/>
					</q-col>
				</q-row>
			</q-col>
		</q-row>
		<q-row v-else>
			<q-col>
				<h1>{{ t('pages.home.header') }}</h1>
			</q-col>
		</q-row>
	</q-page>
</template>

<script setup lang="ts">
import Log from 'consola';
import { useSettingsStore } from '~/store';

const { t } = useI18n();
const settingsStore = useSettingsStore();

const skipSetup = () => {
	Log.info('Setup process skipped');
	settingsStore.generalSettings.firstTimeSetup = false;
};
</script>
