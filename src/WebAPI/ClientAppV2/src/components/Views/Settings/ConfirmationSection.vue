<template>
	<q-section>
		<template #header> {{ $t('pages.settings.ui.confirmation-settings.header') }}</template>
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
									color="red"
									checked-icon="check"
									unchecked-icon="clear"
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
									color="red"
									checked-icon="check"
									unchecked-icon="clear"
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
									color="red"
									checked-icon="check"
									unchecked-icon="clear"
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
									color="red"
									checked-icon="check"
									unchecked-icon="clear"
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
import Log from 'consola';
import { ref } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { SettingsService } from '@service';
import { ConfirmationSettingsDTO } from '@dto/mainApi';

const value = ref(true);

const askDownloadMovieConfirmation = ref(false);
const askDownloadTvShowConfirmation = ref(false);
const askDownloadSeasonConfirmation = ref(false);
const askDownloadEpisodeConfirmation = ref(false);

const updateSettings = (key: keyof ConfirmationSettingsDTO, state: boolean): void => {
	Log.info(`Updating settings with key ${key} and value ${state}`);
	useSubscription(SettingsService.updateConfirmationSetting(key, state).subscribe());
};

onMounted(() => {
	useSubscription(
		SettingsService.getAskDownloadMovieConfirmation().subscribe((data) => {
			Log.info(`getAskDownloadMovieConfirmation with value ${data}`);
			askDownloadMovieConfirmation.value = data;
		}),
	);
	useSubscription(
		SettingsService.getAskDownloadTvShowConfirmation().subscribe((data) => {
			askDownloadTvShowConfirmation.value = data;
		}),
	);
	useSubscription(
		SettingsService.getAskDownloadSeasonConfirmation().subscribe((data) => {
			askDownloadSeasonConfirmation.value = data;
		}),
	);
	useSubscription(
		SettingsService.getAskDownloadEpisodeConfirmation().subscribe((data) => {
			askDownloadEpisodeConfirmation.value = data;
		}),
	);
});
</script>
