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
import { SettingsModelDTO } from '@dto/mainApi';
import SettingsService from '@service/settingsService';
import HelpIcon from '@components/Help/HelpIcon.vue';
import DirectoryBrowser from './components/DirectoryBrowser.vue';

@Component({
	components: {
		DirectoryBrowser,
		HelpIcon,
	},
})
export default class UiSettings extends Vue {
	settings: SettingsModelDTO | null = null;

	get askDownloadMovieConfirmation(): boolean {
		return this.settings?.userInterfaceSettings?.confirmationSettings?.askDownloadMovieConfirmation ?? false;
	}

	set askDownloadMovieConfirmation(value: boolean) {
		if (this.settings?.userInterfaceSettings?.confirmationSettings) {
			if (this.settings?.userInterfaceSettings?.confirmationSettings.askDownloadMovieConfirmation !== value) {
				this.settings.userInterfaceSettings.confirmationSettings.askDownloadMovieConfirmation = value;
				this.updateSettings();
			}
		}
	}

	get askDownloadTvShowConfirmation(): boolean {
		return this.settings?.userInterfaceSettings?.confirmationSettings?.askDownloadTvShowConfirmation ?? false;
	}

	set askDownloadTvShowConfirmation(value: boolean) {
		if (this.settings?.userInterfaceSettings?.confirmationSettings) {
			if (this.settings?.userInterfaceSettings?.confirmationSettings.askDownloadTvShowConfirmation !== value) {
				this.settings.userInterfaceSettings.confirmationSettings.askDownloadTvShowConfirmation = value;
				this.updateSettings();
			}
		}
	}

	get askDownloadSeasonConfirmation(): boolean {
		return this.settings?.userInterfaceSettings?.confirmationSettings?.askDownloadSeasonConfirmation ?? false;
	}

	set askDownloadSeasonConfirmation(value: boolean) {
		if (this.settings?.userInterfaceSettings?.confirmationSettings) {
			if (this.settings?.userInterfaceSettings?.confirmationSettings.askDownloadSeasonConfirmation !== value) {
				this.settings.userInterfaceSettings.confirmationSettings.askDownloadSeasonConfirmation = value;
				this.updateSettings();
			}
		}
	}

	get askDownloadEpisodeConfirmation(): boolean {
		return this.settings?.userInterfaceSettings?.confirmationSettings?.askDownloadEpisodeConfirmation ?? false;
	}

	set askDownloadEpisodeConfirmation(value: boolean) {
		if (this.settings?.userInterfaceSettings?.confirmationSettings) {
			if (this.settings?.userInterfaceSettings?.confirmationSettings.askDownloadEpisodeConfirmation !== value) {
				this.settings.userInterfaceSettings.confirmationSettings.askDownloadEpisodeConfirmation = value;
				this.updateSettings();
			}
		}
	}

	updateSettings(): void {
		if (this.settings) {
			SettingsService.updateSettings(this.settings);
		}
	}

	created(): void {
		SettingsService.getSettings().subscribe((data) => {
			this.settings = data;
		});
	}
}
</script>
