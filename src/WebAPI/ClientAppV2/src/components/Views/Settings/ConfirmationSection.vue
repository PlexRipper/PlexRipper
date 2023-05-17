<template>
	<q-section>
		<template #header> {{ t('pages.settings.ui.confirmation-settings.header') }}</template>
		<q-row no-gutters>
			<q-col>
				<q-markup-table flat>
					<tbody>
						<!--	Ask Download Movie Confirmation	-->
						<tr>
							<td style="width: 30%">
								<help-icon help-id="help.settings.ui.confirmation-settings.download-movie-confirmation" />
							</td>
							<td>
								<q-toggle
									:model-value="askDownloadMovieConfirmation"
									size="lg"
									@update:model-value="updateSettings('askDownloadMovieConfirmation', $event)" />
							</td>
						</tr>
						<!--	Ask Download TvShow Confirmation	-->
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.confirmation-settings.download-tvshow-confirmation" />
							</td>
							<td>
								<q-toggle
									:model-value="askDownloadTvShowConfirmation"
									size="lg"
									@update:model-value="updateSettings('askDownloadTvShowConfirmation', $event)" />
							</td>
						</tr>
						<!--	Ask Download Season Confirmation	-->
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.confirmation-settings.download-season-confirmation" />
							</td>
							<td>
								<q-toggle
									:model-value="askDownloadSeasonConfirmation"
									size="lg"
									@update:model-value="updateSettings('askDownloadSeasonConfirmation', $event)" />
							</td>
						</tr>
						<!--	Ask Download Episode Confirmation	-->
						<tr>
							<td>
								<help-icon help-id="help.settings.ui.confirmation-settings.download-episode-confirmation" />
							</td>
							<td>
								<q-toggle
									:model-value="askDownloadEpisodeConfirmation"
									size="lg"
									@update:model-value="updateSettings('askDownloadEpisodeConfirmation', $event)" />
							</td>
						</tr>
					</tbody>
				</q-markup-table>
			</q-col>
		</q-row>
	</q-section>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { set } from '@vueuse/core';
import { SettingsService } from '@service';
import { ConfirmationSettingsDTO } from '@dto/mainApi';

const { t } = useI18n();
const askDownloadMovieConfirmation = ref(false);
const askDownloadTvShowConfirmation = ref(false);
const askDownloadSeasonConfirmation = ref(false);
const askDownloadEpisodeConfirmation = ref(false);

const updateSettings = (key: keyof ConfirmationSettingsDTO, state: boolean): void => {
	useSubscription(SettingsService.updateConfirmationSetting(key, state).subscribe());
};

onMounted(() => {
	useSubscription(
		SettingsService.getAskDownloadMovieConfirmation().subscribe((data) => {
			set(askDownloadMovieConfirmation, data);
		}),
	);
	useSubscription(
		SettingsService.getAskDownloadTvShowConfirmation().subscribe((data) => {
			set(askDownloadTvShowConfirmation, data);
		}),
	);
	useSubscription(
		SettingsService.getAskDownloadSeasonConfirmation().subscribe((data) => {
			set(askDownloadSeasonConfirmation, data);
		}),
	);
	useSubscription(
		SettingsService.getAskDownloadEpisodeConfirmation().subscribe((data) => {
			set(askDownloadEpisodeConfirmation, data);
		}),
	);
});
</script>
