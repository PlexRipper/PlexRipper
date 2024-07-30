<template>
	<QPage>
		<QRow v-if="settingsStore.generalSettings.firstTimeSetup">
			<QCol cols="12">
				<h2>{{ t('pages.home.setup-question') }}</h2>
				<QRow justify="center">
					<QCol cols="3">
						<NavigationSkipSetupButton
							block
							@click="skipSetup()"
						/>
					</QCol>
					<QCol cols="3">
						<GoToButton
							:label="$t('general.commands.go-to-setup-page')"
							block
							to="/setup"
							color="positive"
						/>
					</QCol>
				</QRow>
			</QCol>
		</QRow>
		<QRow v-else>
			<QCol>
				<h1>{{ t('pages.home.header') }}</h1>
			</QCol>
		</QRow>
	</QPage>
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
