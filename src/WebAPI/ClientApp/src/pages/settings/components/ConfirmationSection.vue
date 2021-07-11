<template>
	<p-section>
		<template #header> {{ $t('pages.settings.ui.confirmation-settings.header') }} </template>
		<v-row no-gutters>
			<v-col>
				<v-simple-table class="section-table">
					<tbody>
						<!--	Ask Download Movie Confirmation	-->
						<tr>
							<td style="width: 30%">
								<help-icon help-id="help.settings.ui.confirmation-settings.download-movie-confirmation" />
							</td>
							<td>
								<p-checkbox :value="askDownloadMovieConfirmation" @input="updateSettings(0, $event)" />
							</td>
						</tr>
						<!--	Ask Download TvShow Confirmation	-->
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.confirmation-settings.download-tvshow-confirmation" />
							</td>
							<td>
								<p-checkbox :value="askDownloadTvShowConfirmation" @input="updateSettings(1, $event)" />
							</td>
						</tr>
						<!--	Ask Download Season Confirmation	-->
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.confirmation-settings.download-season-confirmation" />
							</td>
							<td>
								<p-checkbox :value="askDownloadSeasonConfirmation" @input="updateSettings(2, $event)" />
							</td>
						</tr>
						<!--	Ask Download Episode Confirmation	-->
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.confirmation-settings.download-episode-confirmation" />
							</td>
							<td>
								<p-checkbox :value="askDownloadEpisodeConfirmation" @input="updateSettings(3, $event)" />
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
import { SettingsService } from '@service';

@Component
export default class ConfirmationSection extends Vue {
	askDownloadMovieConfirmation: boolean = false;
	askDownloadTvShowConfirmation: boolean = false;
	askDownloadSeasonConfirmation: boolean = false;
	askDownloadEpisodeConfirmation: boolean = false;

	updateSettings(index: number, state: boolean): void {
		SettingsService.updateConfirmationSettings({
			askDownloadMovieConfirmation: index === 0 ? state : this.askDownloadMovieConfirmation,
			askDownloadTvShowConfirmation: index === 1 ? state : this.askDownloadTvShowConfirmation,
			askDownloadSeasonConfirmation: index === 2 ? state : this.askDownloadSeasonConfirmation,
			askDownloadEpisodeConfirmation: index === 3 ? state : this.askDownloadEpisodeConfirmation,
		});
	}

	mounted(): void {
		this.$subscribeTo(SettingsService.getConfirmationSettings(), (uiSettings) => {
			if (uiSettings) {
				this.askDownloadMovieConfirmation = uiSettings.askDownloadMovieConfirmation;
				this.askDownloadTvShowConfirmation = uiSettings.askDownloadTvShowConfirmation;
				this.askDownloadSeasonConfirmation = uiSettings.askDownloadSeasonConfirmation;
				this.askDownloadEpisodeConfirmation = uiSettings.askDownloadEpisodeConfirmation;
			}
		});
	}
}
</script>
