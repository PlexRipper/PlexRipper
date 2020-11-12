<template>
	<v-container fluid>
		<v-row>
			<v-col>
				<v-sheet width="100%" class="pa-4">
					<v-row>
						<v-col>
							<h1>Confirmation Settings</h1>
							<v-divider />
						</v-col>
					</v-row>

					<!--	Ask Download Movie Confirmation	-->
					<v-row>
						<v-col cols="4">
							<help-icon label="Ask Download Movie Confirmation" />
						</v-col>
						<v-col cols="8">
							<v-checkbox v-model="askDownloadMovieConfirmation" class="mt-0"></v-checkbox>
						</v-col>
					</v-row>

					<!--	Ask Download TvShow Confirmation	-->
					<v-row>
						<v-col cols="4">
							<help-icon label="Ask Download TvShow Confirmation" />
						</v-col>
						<v-col cols="8">
							<v-checkbox v-model="askDownloadTvShowConfirmation" class="mt-0"></v-checkbox>
						</v-col>
					</v-row>

					<!--	Ask Download Season Confirmation	-->
					<v-row>
						<v-col cols="4">
							<help-icon label="Ask Download Season Confirmation" />
						</v-col>
						<v-col cols="8">
							<v-checkbox v-model="askDownloadSeasonConfirmation" class="mt-0"></v-checkbox>
						</v-col>
					</v-row>

					<!--	Ask Download Episode Confirmation	-->
					<v-row>
						<v-col cols="4">
							<help-icon label="Ask Download Episode Confirmation" />
						</v-col>
						<v-col cols="8">
							<v-checkbox v-model="askDownloadEpisodeConfirmation" class="mt-0"></v-checkbox>
						</v-col>
					</v-row>
				</v-sheet>
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import { settingsStore as SettingsStore } from '@/store';
import HelpIcon from '@components/Help/HelpIcon.vue';
import DirectoryBrowser from '@components/General/DirectoryBrowser.vue';

@Component({
	components: {
		DirectoryBrowser,
		HelpIcon,
	},
})
export default class UiSettings extends Vue {
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
