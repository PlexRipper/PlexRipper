import Log from 'consola';
import { Observable, of } from 'rxjs';
import { distinctUntilChanged, filter, map, switchMap, take, tap } from 'rxjs/operators';
import { Context } from '@nuxt/types';
import { isEqual } from 'lodash-es';
import {
	ConfirmationSettingsDTO,
	DateTimeSettingsDTO,
	DisplaySettingsDTO,
	DownloadManagerSettingsDTO,
	GeneralSettingsDTO,
	LanguageSettingsDTO,
	PlexServerSettingsModel,
	ServerSettingsDTO,
	SettingsModelDTO,
	ViewMode,
} from '@dto/mainApi';
import { BaseService } from '@service';
import { getSettings, updateSettings } from '@api/settingsApi';
import IStoreState from '@interfaces/service/IStoreState';
import ISetupResult from '@interfaces/service/ISetupResult';

export class SettingsService extends BaseService {
	// region Constructor and Setup
	public constructor() {
		super('SettingsService', {
			// Note: Each service file can only have "unique" state slices which are not also used in other service files
			stateSliceSelector: (state: IStoreState) => {
				return {
					generalSettings: state.generalSettings,
					confirmationSettings: state.confirmationSettings,
					dateTimeSettings: state.dateTimeSettings,
					displaySettings: state.displaySettings,
					downloadManagerSettings: state.downloadManagerSettings,
					languageSettings: state.languageSettings,
					serverSettings: state.serverSettings,
				};
			},
		});
	}

	public setup(nuxtContext: Context): Observable<ISetupResult> {
		super.setup(nuxtContext);

		this.getFirstTimeSetup().subscribe((state) => {
			if (state === null) {
				return;
			}
			Log.info('Redirecting to the setup page');
			if (state && !nuxtContext.route.path.includes('setup')) {
				return nuxtContext.redirect('/setup');
			}
		});
		// On app load, request the settings once
		return this.fetchSettings().pipe(
			switchMap(() => of({ name: this._name, isSuccess: true })),
			take(1),
		);
	}

	// endregion

	// region Private

	private getSettingsModel(): SettingsModelDTO {
		return {
			generalSettings: this.getSettingsModule('generalSettings'),
			confirmationSettings: this.getSettingsModule('confirmationSettings'),
			dateTimeSettings: this.getSettingsModule('dateTimeSettings'),
			displaySettings: this.getSettingsModule('displaySettings'),
			downloadManagerSettings: this.getSettingsModule('downloadManagerSettings'),
			languageSettings: this.getSettingsModule('languageSettings'),
			serverSettings: this.getSettingsModule('serverSettings'),
		};
	}

	private setSettingsModel(settings: SettingsModelDTO): void {
		this.setState(
			{
				generalSettings: settings.generalSettings,
				confirmationSettings: settings.confirmationSettings,
				dateTimeSettings: settings.dateTimeSettings,
				displaySettings: settings.displaySettings,
				downloadManagerSettings: settings.downloadManagerSettings,
				languageSettings: settings.languageSettings,
				serverSettings: settings.serverSettings,
			},
			'Set SettingsModel to all settingsModule',
		);
	}

	private getSettingsModule(settingModule: keyof SettingsModelDTO): any {
		return this.getStateSliceProperty(settingModule);
	}

	private sendSettingsToApi(): void {
		updateSettings(this.getSettingsModel()).pipe(take(1)).subscribe();
	}

	// endregion

	// region Fetch
	public fetchSettings(): Observable<SettingsModelDTO | null> {
		return getSettings().pipe(
			switchMap((settingsResult) => of(settingsResult?.value ?? null)),
			tap((settings) => {
				if (settings) {
					this.setSettingsModel(settings);
				}
			}),
		);
	}

	// endregion

	// region GeneralSettings

	public updateGeneralSettings<T>(setting: keyof GeneralSettingsDTO, value: T): void {
		const settings = {
			generalSettings: {
				...this.getSettingsModule('generalSettings'),
				[setting]: value,
			},
		} as SettingsModelDTO;

		this.setState(settings, `Update GeneralSettings: ${setting} with value: ${value}`);
		this.sendSettingsToApi();
	}

	public getFirstTimeSetup(): Observable<boolean> {
		return this.stateChanged.pipe(
			map((x) => x?.generalSettings.firstTimeSetup),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	public getActiveAccountId(): Observable<number> {
		return this.stateChanged.pipe(
			map((x) => x?.generalSettings.activeAccountId),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	// endregion

	// region LanguageSettings
	public updateLanguageSettings<T>(setting: keyof LanguageSettingsDTO, value: T): void {
		const settings = {
			languageSettings: {
				...this.getSettingsModule('languageSettings'),
				[setting]: value,
			},
		} as SettingsModelDTO;

		this.setState(settings, `Update LanguageSettings: ${setting} with value: ${value}`);
		this.sendSettingsToApi();
	}

	public getLanguage(): Observable<string> {
		return this.stateChanged.pipe(
			map((x) => x?.languageSettings.language),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	// endregion

	// region ConfirmationSettings

	public updateConfirmationSetting<T>(setting: keyof ConfirmationSettingsDTO, value: T): void {
		const settings = {
			confirmationSettings: {
				...this.getSettingsModule('confirmationSettings'),
				[setting]: value,
			},
		} as SettingsModelDTO;

		this.setState(settings, `Update ConfirmationSetting: ${setting} with value: ${value}`);
		this.sendSettingsToApi();
	}

	public getAskDownloadMovieConfirmation(): Observable<boolean> {
		return this.stateChanged.pipe(
			map((x) => x?.confirmationSettings.askDownloadMovieConfirmation),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	public getAskDownloadTvShowConfirmation(): Observable<boolean> {
		return this.stateChanged.pipe(
			map((x) => x?.confirmationSettings.askDownloadTvShowConfirmation),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	public getAskDownloadSeasonConfirmation(): Observable<boolean> {
		return this.stateChanged.pipe(
			map((x) => x?.confirmationSettings.askDownloadSeasonConfirmation),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	public getAskDownloadEpisodeConfirmation(): Observable<boolean> {
		return this.stateChanged.pipe(
			map((x) => x?.confirmationSettings.askDownloadEpisodeConfirmation),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	// endregion

	// region DateTimeSettings

	public updateDateTimeSetting<T>(setting: keyof DateTimeSettingsDTO, value: T): void {
		const x = {
			dateTimeSettings: {
				...this.getSettingsModule('dateTimeSettings'),
				[setting]: value,
			},
		} as SettingsModelDTO;

		this.setState(x, `Update DateTimeSetting: ${setting} with value: ${value}`);
		this.sendSettingsToApi();
	}

	public getShortDateFormat(): Observable<string> {
		return this.stateChanged.pipe(
			map((x) => x?.dateTimeSettings.shortDateFormat),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	public getLongDateFormat(): Observable<string> {
		return this.stateChanged.pipe(
			map((x) => x?.dateTimeSettings.longDateFormat),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	public getTimeFormat(): Observable<string> {
		return this.stateChanged.pipe(
			map((x) => x?.dateTimeSettings.timeFormat),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	public getTimeZone(): Observable<string> {
		return this.stateChanged.pipe(
			map((x) => x?.dateTimeSettings.timeZone),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	public getShowRelativeDates(): Observable<boolean> {
		return this.stateChanged.pipe(
			map((x) => x?.dateTimeSettings.showRelativeDates),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	// endregion

	// region DisplaySettings

	public updateDisplaySettings<T>(setting: keyof DisplaySettingsDTO, value: T): void {
		const x = {
			displaySettings: {
				...this.getSettingsModule('displaySettings'),
				[setting]: value,
			},
		} as SettingsModelDTO;

		this.setState(x, `Update DisplaySettings: ${setting} with value: ${value}`);
		this.sendSettingsToApi();
	}

	public getMovieViewMode(): Observable<ViewMode> {
		return this.stateChanged.pipe(
			map((x) => x?.displaySettings.movieViewMode),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	public getTvShowViewMode(): Observable<ViewMode> {
		return this.stateChanged.pipe(
			map((x) => x?.displaySettings.tvShowViewMode),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	// endregion

	// region DownloadManagerSettings
	public updateDownloadManagerSetting<T>(setting: keyof DownloadManagerSettingsDTO, value: T): void {
		const x = {
			downloadManagerSettings: {
				...this.getSettingsModule('downloadManagerSettings'),
				[setting]: value,
			},
		} as unknown as SettingsModelDTO;

		this.setState(x, `Update DownloadManagerSetting: ${setting} with value: ${value}`);
		this.sendSettingsToApi();
	}

	public getDownloadSegments(): Observable<number> {
		return this.stateChanged.pipe(
			map((x) => x?.downloadManagerSettings.downloadSegments),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	// endregion

	// region ServerSettings

	public updateServerSettings(value: PlexServerSettingsModel): void {
		const data = this.getSettingsModule('serverSettings').data as PlexServerSettingsModel[];
		const settings: ServerSettingsDTO = {
			data: [...data.filter((x) => x.machineIdentifier !== value.machineIdentifier), value],
		};

		this.setState(
			{ serverSettings: settings },
			`Update ServerSettings for server ${value.plexServerName} with machine id: ${value.machineIdentifier}`,
		);
		this.sendSettingsToApi();
	}

	public getServerSettings(machineIdentifier: string): Observable<PlexServerSettingsModel> {
		return this.stateChanged.pipe(
			map((x) => x?.serverSettings.data.find((y) => y.machineIdentifier === machineIdentifier)),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	// endregion
}

const settingsService = new SettingsService();
export default settingsService;
