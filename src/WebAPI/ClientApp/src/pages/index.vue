<template>
	<QPage>
		<QRow v-if="settingsStore.generalSettings.firstTimeSetup">
			<QCol cols="12">
				<QText
					align="center"
					size="h5">
					{{ $t('pages.home.setup-question') }}
				</QText>
				<QRow
					justify="center"
					gutter="md">
					<QCol cols="3">
						<NavigationSkipSetupButton
							block
							@click="skipSetup()" />
					</QCol>
					<QCol cols="3">
						<GoToButton
							:label="$t('general.commands.go-to-setup-page')"
							block
							to="/setup"
							color="positive" />
					</QCol>
				</QRow>
			</QCol>
		</QRow>
		<QRow v-else>
			<QCol>
				<SearchBar />
			</QCol>
		</QRow>
	</QPage>
</template>

<script setup lang="ts">
import Log from 'consola';
import { useSettingsStore } from '@store';

const settingsStore = useSettingsStore();

const skipSetup = () => {
	Log.info('Setup process skipped');
	settingsStore.generalSettings.firstTimeSetup = false;
};
</script>
