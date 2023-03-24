<template>
	<p-section>
		<template #header> {{ $t('pages.settings.ui.confirmation-settings.header') }}</template>
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
import Log from 'consola';
import { useSubscription } from '@vueuse/rxjs';
import { SettingsService } from '@service';
import { ConfirmationSettingsDTO } from '@dto/mainApi';

@Component
export default class ConfirmationSection extends Vue {
	askDownloadMovieConfirmation: boolean = false;
	askDownloadTvShowConfirmation: boolean = false;
	askDownloadSeasonConfirmation: boolean = false;
	askDownloadEpisodeConfirmation: boolean = false;

	updateSettings(index: number, state: boolean): void {
		let key: keyof ConfirmationSettingsDTO | null = null;
		switch (index) {
			case 0:
				key = 'askDownloadMovieConfirmation';
				break;
			case 1:
				key = 'askDownloadTvShowConfirmation';
				break;
			case 2:
				key = 'askDownloadSeasonConfirmation';
				break;
			case 3:
				key = 'askDownloadEpisodeConfirmation';
				break;
			default:
				Log.error(`Failed to update settings with index ${index} and value ${state}`);
				key = null;
		}
		if (key) {
			useSubscription(SettingsService.updateConfirmationSetting(key, state).subscribe());
		}
	}

	mounted(): void {
		useSubscription(
			SettingsService.getAskDownloadMovieConfirmation().subscribe((value) => {
				this.askDownloadMovieConfirmation = value;
			}),
		);
		useSubscription(
			SettingsService.getAskDownloadTvShowConfirmation().subscribe((value) => {
				this.askDownloadTvShowConfirmation = value;
			}),
		);
		useSubscription(
			SettingsService.getAskDownloadSeasonConfirmation().subscribe((value) => {
				this.askDownloadSeasonConfirmation = value;
			}),
		);
		useSubscription(
			SettingsService.getAskDownloadEpisodeConfirmation().subscribe((value) => {
				this.askDownloadEpisodeConfirmation = value;
			}),
		);
	}
}
</script>
