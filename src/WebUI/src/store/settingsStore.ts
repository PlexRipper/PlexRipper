import Log from 'consola';
import { Module, Mutation, VuexModule } from 'vuex-module-decorators';
import type { SettingsModel } from '@dto/mainApi';
import SettingsService from '@service/settingsService';

// Doc: https://typescript.nuxtjs.org/cookbook/store.html#class-based
@Module({ name: 'settingsStore', stateFactory: true })
export default class SettingsStore extends VuexModule {
	settings: SettingsModel | null = null;

	@Mutation
	setSettings(settings: SettingsModel) {
		Log.info('Vuex store was updated with settings');
		this.settings = settings;
	}

	get getSettings(): SettingsModel | null {
		return this.settings;
	}

	// region Date Time Settings
	get shortDateFormat(): string {
		return this.settings?.userInterfaceSettings?.dateTimeSettings?.shortDateFormat ?? '';
	}

	get longDateFormat(): string {
		return this.settings?.userInterfaceSettings?.dateTimeSettings?.longDateFormat ?? '';
	}

	get timeFormat(): string {
		return this.settings?.userInterfaceSettings?.dateTimeSettings?.timeFormat ?? '';
	}

	get showRelativeDates(): boolean {
		return this.settings?.userInterfaceSettings?.dateTimeSettings?.showRelativeDates ?? false;
	}

	@Mutation
	setShortDateFormat(value: string) {
		if (this.settings?.userInterfaceSettings?.dateTimeSettings) {
			this.settings.userInterfaceSettings.dateTimeSettings.shortDateFormat = value;
			SettingsService.updateSettings(this.settings);
		}
	}

	@Mutation
	setLongDateFormat(value: string) {
		if (this.settings?.userInterfaceSettings?.dateTimeSettings) {
			this.settings.userInterfaceSettings.dateTimeSettings.longDateFormat = value;
			SettingsService.updateSettings(this.settings);
		}
	}

	@Mutation
	setTimeFormat(value: string) {
		if (this.settings?.userInterfaceSettings?.dateTimeSettings) {
			this.settings.userInterfaceSettings.dateTimeSettings.timeFormat = value;
			SettingsService.updateSettings(this.settings);
		}
	}

	@Mutation
	setShowRelativeDates(value: boolean) {
		if (this.settings?.userInterfaceSettings?.dateTimeSettings) {
			this.settings.userInterfaceSettings.dateTimeSettings.showRelativeDates = value;
			SettingsService.updateSettings(this.settings);
		}
	}

	// endregion

	// region Confirmation Settings
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

	// region Advanced

	get downloadSegments(): number {
		return this.settings?.advancedSettings?.downloadManager?.downloadSegments ?? 4;
	}

	@Mutation
	setDownloadSegments(value: number) {
		if (this.settings?.advancedSettings?.downloadManager?.downloadSegments) {
			this.settings.advancedSettings.downloadManager.downloadSegments = value;
			SettingsService.updateSettings(this.settings);
		}
	}
	// endregion
}
