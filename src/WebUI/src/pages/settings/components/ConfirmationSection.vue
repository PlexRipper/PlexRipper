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
import { settingsStore } from '~/store';

@Component
export default class ConfirmationSection extends Vue {
	get askDownloadMovieConfirmation(): boolean {
		return settingsStore.askDownloadMovieConfirmation;
	}

	set askDownloadMovieConfirmation(value: boolean) {
		settingsStore.setAskDownloadMovieConfirmation(value);
	}

	get askDownloadTvShowConfirmation(): boolean {
		return settingsStore.askDownloadTvShowConfirmation;
	}

	set askDownloadTvShowConfirmation(value: boolean) {
		settingsStore.setAskDownloadTvShowConfirmation(value);
	}

	get askDownloadSeasonConfirmation(): boolean {
		return settingsStore.askDownloadSeasonConfirmation;
	}

	set askDownloadSeasonConfirmation(value: boolean) {
		settingsStore.setAskDownloadSeasonConfirmation(value);
	}

	get askDownloadEpisodeConfirmation(): boolean {
		return settingsStore.askDownloadEpisodeConfirmation;
	}

	set askDownloadEpisodeConfirmation(value: boolean) {
		settingsStore.setAskDownloadEpisodeConfirmation(value);
	}
}
</script>
