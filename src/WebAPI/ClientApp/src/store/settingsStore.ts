import Log from 'consola';
import { Module, Mutation, VuexModule } from 'vuex-module-decorators';
import type { SettingsModel } from '@dto/mainApi';
import SettingsService from '@state/settingsService';
import { ViewMode } from '@dto/mainApi';

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

	// region General
	get firstTimeSetup(): boolean {
		return this.settings?.firstTimeSetup ?? false;
	}

	get activeAccountId(): number {
		return this.settings?.accountSettings?.activeAccountId ?? 0;
	}

	@Mutation
	setFirstTimeSetup(value: boolean) {
		if (this.settings?.firstTimeSetup) {
			this.settings.firstTimeSetup = value;
			SettingsService.updateSettings();
		}
	}

	@Mutation
	setActiveAccountId(value: number) {
		if (this.settings?.accountSettings) {
			this.settings.accountSettings.activeAccountId = value;
			SettingsService.updateSettings();
		}
	}

	// endregion

	// region ViewMode
	get movieViewMode(): ViewMode {
		return this.settings?.userInterfaceSettings?.displaySettings?.movieViewMode ?? ViewMode.Poster;
	}

	get tvShowViewMode(): ViewMode {
		return this.settings?.userInterfaceSettings?.displaySettings?.tvShowViewMode ?? ViewMode.Poster;
	}

	@Mutation
	setMovieViewMode(value: ViewMode) {
		if (this.settings?.userInterfaceSettings?.displaySettings) {
			this.settings.userInterfaceSettings.displaySettings.movieViewMode = value;
			SettingsService.updateSettings();
		}
	}

	@Mutation
	setTvShowViewMode(value: ViewMode) {
		if (this.settings?.userInterfaceSettings?.displaySettings) {
			this.settings.userInterfaceSettings.displaySettings.tvShowViewMode = value;
			SettingsService.updateSettings();
		}
	}
	// endregion

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

	get timeZone(): string {
		return this.settings?.userInterfaceSettings?.dateTimeSettings?.timeZone ?? '';
	}

	get showRelativeDates(): boolean {
		return this.settings?.userInterfaceSettings?.dateTimeSettings?.showRelativeDates ?? false;
	}

	@Mutation
	setShortDateFormat(value: string) {
		if (this.settings?.userInterfaceSettings?.dateTimeSettings) {
			this.settings.userInterfaceSettings.dateTimeSettings.shortDateFormat = value;
			SettingsService.updateSettings();
		}
	}

	@Mutation
	setLongDateFormat(value: string) {
		if (this.settings?.userInterfaceSettings?.dateTimeSettings) {
			this.settings.userInterfaceSettings.dateTimeSettings.longDateFormat = value;
			SettingsService.updateSettings();
		}
	}

	@Mutation
	setTimeFormat(value: string) {
		if (this.settings?.userInterfaceSettings?.dateTimeSettings) {
			this.settings.userInterfaceSettings.dateTimeSettings.timeFormat = value;
			SettingsService.updateSettings();
		}
	}

	@Mutation
	setTimeZone(value: string) {
		if (this.settings?.userInterfaceSettings?.dateTimeSettings) {
			this.settings.userInterfaceSettings.dateTimeSettings.timeZone = value;
			SettingsService.updateSettings();
		}
	}

	@Mutation
	setShowRelativeDates(value: boolean) {
		if (this.settings?.userInterfaceSettings?.dateTimeSettings) {
			this.settings.userInterfaceSettings.dateTimeSettings.showRelativeDates = value;
			SettingsService.updateSettings();
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
		Log.debug('settings', this.settings);
		return this.settings?.userInterfaceSettings?.confirmationSettings?.askDownloadEpisodeConfirmation ?? false;
	}

	@Mutation
	setAskDownloadMovieConfirmation(value: boolean) {
		if (this.settings?.userInterfaceSettings?.confirmationSettings) {
			this.settings.userInterfaceSettings.confirmationSettings.askDownloadMovieConfirmation = value;
			SettingsService.updateSettings();
		}
	}

	@Mutation
	setAskDownloadTvShowConfirmation(value: boolean) {
		if (this.settings?.userInterfaceSettings?.confirmationSettings) {
			this.settings.userInterfaceSettings.confirmationSettings.askDownloadTvShowConfirmation = value;
			SettingsService.updateSettings();
		}
	}

	@Mutation
	setAskDownloadSeasonConfirmation(value: boolean) {
		if (this.settings?.userInterfaceSettings?.confirmationSettings) {
			this.settings.userInterfaceSettings.confirmationSettings.askDownloadSeasonConfirmation = value;
			SettingsService.updateSettings();
		}
	}

	@Mutation
	setAskDownloadEpisodeConfirmation(value: boolean) {
		if (this.settings?.userInterfaceSettings?.confirmationSettings) {
			this.settings.userInterfaceSettings.confirmationSettings.askDownloadEpisodeConfirmation = value;
			SettingsService.updateSettings();
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
			SettingsService.updateSettings();
		}
	}
	// endregion
}
