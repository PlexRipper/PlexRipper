<template>
	<p-section>
		<v-row no-gutters>
			<v-col>
				<h1>Confirmation Settings</h1>
				<v-divider />
			</v-col>
		</v-row>
		<v-row no-gutters>
			<v-col>
				<v-simple-table class="section-table">
					<tbody>
						<!--	Ask Download Movie Confirmation	-->
						<tr>
							<td style="width: 35%">
								<help-icon help-id="help.settings.ui.confirmation-settings.download-movie-confirmation" />
							</td>
							<td>
								<p-checkbox v-model="askDownloadMovieConfirmation" />
							</td>
						</tr>
						<!--	Ask Download TvShow Confirmation	-->
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.confirmation-settings.download-tvshow-confirmation" />
							</td>
							<td>
								<p-checkbox v-model="askDownloadTvShowConfirmation" />
							</td>
						</tr>
						<!--	Ask Download Season Confirmation	-->
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.confirmation-settings.download-season-confirmation" />
							</td>
							<td>
								<p-checkbox v-model="askDownloadSeasonConfirmation" />
							</td>
						</tr>
						<!--	Ask Download Episode Confirmation	-->
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.confirmation-settings.download-episode-confirmation" />
							</td>
							<td>
								<p-checkbox v-model="askDownloadEpisodeConfirmation" />
							</td>
						</tr>
					</tbody>
				</v-simple-table>
			</v-col>
		</v-row>
	</p-section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { settingsStore as SettingsStore } from '~/store';

@Component
export default class ConfirmationSection extends Vue {
	get askDownloadMovieConfirmation(): boolean {
		return SettingsStore.askDownloadMovieConfirmation;
	}

	set askDownloadMovieConfirmation(value: boolean) {
		SettingsStore.setAskDownloadMovieConfirmation(value);
	}

	get askDownloadTvShowConfirmation(): boolean {
		return SettingsStore.askDownloadTvShowConfirmation;
	}

	set askDownloadTvShowConfirmation(value: boolean) {
		SettingsStore.setAskDownloadTvShowConfirmation(value);
	}

	get askDownloadSeasonConfirmation(): boolean {
		return SettingsStore.askDownloadSeasonConfirmation;
	}

	set askDownloadSeasonConfirmation(value: boolean) {
		SettingsStore.setAskDownloadSeasonConfirmation(value);
	}

	get askDownloadEpisodeConfirmation(): boolean {
		return SettingsStore.askDownloadEpisodeConfirmation;
	}

	set askDownloadEpisodeConfirmation(value: boolean) {
		SettingsStore.setAskDownloadEpisodeConfirmation(value);
	}
}
</script>
