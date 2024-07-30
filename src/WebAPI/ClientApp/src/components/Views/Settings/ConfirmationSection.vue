<template>
	<q-section>
		<template #header>
			{{ t('pages.settings.ui.confirmation-settings.header') }}
		</template>
		<!--	Ask Download Movie Confirmation	-->
		<help-row help-id="help.settings.ui.confirmation-settings.download-movie-confirmation">
			<q-toggle
				v-model:model-value="settingsStore.confirmationSettings.askDownloadMovieConfirmation"
				size="lg"
				data-cy="ask-download-movie-confirmation"
			/>
		</help-row>
		<!--	Ask Download TvShow Confirmation	-->
		<help-row help-id="help.settings.ui.confirmation-settings.download-tvshow-confirmation">
			<q-toggle
				v-model:model-value="settingsStore.confirmationSettings.askDownloadTvShowConfirmation"
				size="lg"
				data-cy="ask-download-tvshow-confirmation"
			/>
		</help-row>
		<!--	Ask Download Season Confirmation	-->
		<help-row help-id="help.settings.ui.confirmation-settings.download-season-confirmation">
			<q-toggle
				v-model:model-value="settingsStore.confirmationSettings.askDownloadSeasonConfirmation"
				size="lg"
				data-cy="ask-download-season-confirmation"
			/>
		</help-row>
		<!--	Ask Download Episode Confirmation	-->
		<help-row help-id="help.settings.ui.confirmation-settings.download-episode-confirmation">
			<q-toggle
				v-model:model-value="settingsStore.confirmationSettings.askDownloadEpisodeConfirmation"
				size="lg"
				data-cy="ask-download-episode-confirmation"
			/>
		</help-row>
	</q-section>
</template>

<script setup lang="ts">
import { useSettingsStore } from '~/store';

const { t } = useI18n();
const settingsStore = useSettingsStore();
</script>
