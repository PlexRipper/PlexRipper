<template>
	<QSection>
		<template #header>
			{{ $t('pages.settings.ui.confirmation-settings.header') }}
		</template>
		<!--	Ask Download Movie Confirmation	-->
		<HelpRow
			:label="$t('help.settings.ui.confirmation-settings.download-movie-confirmation.label')"
			:title="$t('help.settings.ui.confirmation-settings.download-movie-confirmation.title')"
			:text="$t('help.settings.ui.confirmation-settings.download-movie-confirmation.text')">
			<q-toggle
				v-model:model-value="settingsStore.confirmationSettings.askDownloadMovieConfirmation"
				size="lg"
				data-cy="ask-download-movie-confirmation" />
		</HelpRow>
		<!--	Ask Download TvShow Confirmation	-->
		<HelpRow
			:label="$t('help.settings.ui.confirmation-settings.download-tvshow-confirmation.label')"
			:title="$t('help.settings.ui.confirmation-settings.download-tvshow-confirmation.title')"
			:text="$t('help.settings.ui.confirmation-settings.download-tvshow-confirmation.text')">
			<q-toggle
				v-model:model-value="settingsStore.confirmationSettings.askDownloadTvShowConfirmation"
				size="lg"
				data-cy="ask-download-tvshow-confirmation" />
		</HelpRow>
		<!--	Ask Download Season Confirmation	-->
		<HelpRow
			:label="$t('help.settings.ui.confirmation-settings.download-season-confirmation.label')"
			:title="$t('help.settings.ui.confirmation-settings.download-season-confirmation.title')"
			:text="$t('help.settings.ui.confirmation-settings.download-season-confirmation.text')">
			<q-toggle
				v-model:model-value="settingsStore.confirmationSettings.askDownloadSeasonConfirmation"
				size="lg"
				data-cy="ask-download-season-confirmation" />
		</HelpRow>
		<!--	Ask Download Episode Confirmation	-->
		<HelpRow
			:label="$t('help.settings.ui.confirmation-settings.download-episode-confirmation.label')"
			:title="$t('help.settings.ui.confirmation-settings.download-episode-confirmation.title')"
			:text="$t('help.settings.ui.confirmation-settings.download-episode-confirmation.text')">
			<q-toggle
				v-model:model-value="settingsStore.confirmationSettings.askDownloadEpisodeConfirmation"
				size="lg"
				data-cy="ask-download-episode-confirmation" />
		</HelpRow>
	</QSection>
</template>

<script setup lang="ts">
import { useSettingsStore } from '@store';

const settingsStore = useSettingsStore();
</script>
