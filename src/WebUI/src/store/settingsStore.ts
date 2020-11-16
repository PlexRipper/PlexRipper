import Log from 'consola';
import { Module, Mutation, VuexModule } from 'vuex-module-decorators';
import type { SettingsModelDTO } from '@dto/mainApi';
import SettingsService from '@service/settingsService';

// Doc: https://typescript.nuxtjs.org/cookbook/store.html#class-based
@Module({ name: 'settingsStore', stateFactory: true })
export default class SettingsStore extends VuexModule {
	settings: SettingsModelDTO | null = null;

	@Mutation
	setSettings(settings: SettingsModelDTO) {
		Log.info('Vuex store was updated with settings');
		this.settings = settings;
	}

	get getSettings(): SettingsModelDTO | null {
		return this.settings;
	}

	// region User Interface Settings
	get askDownloadMovieConfirmation(): boolean {
		return this.settings?.userInterfaceSettings?.confirmationSettings?.askDownloadMovieConfirmation ?? false;
	}

	get askDownloadTvShowConfirmation(): boolean {
		return this.settings?.userInterfaceSettings?.confirmationSettings?.askDownloadTvShowConfirmation ?? false;
	}

	get askDownloadSeasonConfirmation(): boolean {
		return this.settings?.userInterfaceSettings?.confirmationSettings?.askDownloadSeasonConfirmation ?? false;
	}

	get askDownloadEpisodeConfirmation(): boolean {
		return this.settings?.userInterfaceSettings?.confirmationSettings?.askDownloadEpisodeConfirmation ?? false;
	}

	@Mutation
	setAskDownloadMovieConfirmation(value: boolean) {
		if (this.settings?.userInterfaceSettings?.confirmationSettings) {
			Log.debug(`Changed setting AskDownloadMovieConfirmation to value ${value}`);
			this.settings.userInterfaceSettings.confirmationSettings.askDownloadMovieConfirmation = value;
			SettingsService.updateSettings(this.settings);
		}
	}

	@Mutation
	setAskDownloadTvShowConfirmation(value: boolean) {
		if (this.settings?.userInterfaceSettings?.confirmationSettings) {
			this.settings.userInterfaceSettings.confirmationSettings.askDownloadTvShowConfirmation = value;
			SettingsService.updateSettings(this.settings);
		}
	}

	@Mutation
	setAskDownloadSeasonConfirmation(value: boolean) {
		if (this.settings?.userInterfaceSettings?.confirmationSettings) {
			this.settings.userInterfaceSettings.confirmationSettings.askDownloadSeasonConfirmation = value;
			SettingsService.updateSettings(this.settings);
		}
	}

	@Mutation
	setAskDownloadEpisodeConfirmation(value: boolean) {
		if (this.settings?.userInterfaceSettings?.confirmationSettings) {
			this.settings.userInterfaceSettings.confirmationSettings.askDownloadEpisodeConfirmation = value;
			SettingsService.updateSettings(this.settings);
		}
	}
	// endregion
}
