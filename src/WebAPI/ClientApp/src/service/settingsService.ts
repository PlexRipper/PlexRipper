import Log from 'consola';
import IStoreState from '@interfaces/IStoreState';
import { Observable, of } from 'rxjs';
import {
	AccountSettingsModelDTO,
	AdvancedSettingsModelDTO,
	ConfirmationSettingsModelDTO,
	DateTimeModelDTO,
	DisplaySettingsModelDTO,
	DownloadManagerModelDTO,
	SettingsModelDTO,
	UserInterfaceSettingsModelDTO,
	ViewMode,
} from '@dto/mainApi';
import { filter, switchMap, take, tap } from 'rxjs/operators';
import { BaseService, GlobalService } from '@service';
import { getSettings, updateSettings } from '@api/settingsApi';
import { Context } from '@nuxt/types';

export class SettingsService extends BaseService {
	// region Constructor and Setup

	public constructor() {
		super({
			stateSliceSelector: (state: IStoreState) => {
				return {
					settings: state.settings,
				};
			},
		});
	}

	public setup(nuxtContext: Context): void {
		super.setup(nuxtContext);

		// On app load, request the settings once
		GlobalService.getAxiosReady()
			.pipe(
				tap(() => Log.debug('Retrieving settings')),
				switchMap(() => this.fetchSettings()),
				take(1),
			)
			.subscribe();

		// On settings service update => send update to back-end
		this.getSettings()
			.pipe(switchMap((settings) => updateSettings(settings)))
			.subscribe();

		this.getFirstTimeSetup().subscribe((state) => {
			if (state === null) {
				return;
			}
			Log.info('Redirecting to the setup page');
			if (state && nuxtContext.route.path !== '/setup') {
				return nuxtContext.redirect('/setup');
			}
		});
	}
	// endregion

	// region Fetch
	public fetchSettings(): Observable<SettingsModelDTO | null> {
		return getSettings().pipe(
			switchMap((settingsResult) => of(settingsResult?.value ?? null)),
			tap((settings) => {
				Log.debug(`SettingsService => Fetch Settings`, settings);
				if (settings) {
					this.setState({ settings }, 'Fetch Settings');
				}
			}),
		);
	}
	// endregion

	// region Settings
	public getSettings(): Observable<SettingsModelDTO> {
		return this.stateChanged.pipe(
			filter((x) => x !== null),
			switchMap((x) => of(x.settings)),
		);
	}

	public updateSettings(): void {
		const settings = this.getState().settings;
		if (settings) {
			updateSettings(settings).subscribe((settings) => {
				if (settings.isSuccess) {
					this.setState({ settings: settings.value });
				}
			});
		} else {
			Log.warn('SettingsService => updateSettings: settings was invalid, will not send as an update.');
		}
	}
	// endregion

	public getFirstTimeSetup(): Observable<boolean> {
		return this.getSettings().pipe(switchMap((x) => of(x.firstTimeSetup)));
	}

	public updateFirstTimeSetup(state: boolean): void {
		const settings = this.getState().settings;
		this.setState({
			...this.getState(),
			settings: {
				...settings,
				firstTimeSetup: state,
			},
		});
	}

	// region UserInterfaceSettings

	public getUserInterfaceSettings(): Observable<UserInterfaceSettingsModelDTO> {
		return this.getSettings().pipe(switchMap((x) => of(x.userInterfaceSettings)));
	}

	public getConfirmationSettings(): Observable<ConfirmationSettingsModelDTO> {
		return this.getUserInterfaceSettings().pipe(switchMap((x) => of(x.confirmationSettings)));
	}

	public getDisplaySettings(): Observable<DisplaySettingsModelDTO> {
		return this.getUserInterfaceSettings().pipe(switchMap((x) => of(x.displaySettings)));
	}

	public getDateTimeSettings(): Observable<DateTimeModelDTO> {
		return this.getUserInterfaceSettings().pipe(switchMap((x) => of(x.dateTimeSettings)));
	}

	public updateConfirmationSettings(confirmationSettings: ConfirmationSettingsModelDTO): void {
		const userInterfaceSettings = this.getState().settings.userInterfaceSettings;
		this.updateUserInterfaceSettings({ ...userInterfaceSettings, confirmationSettings });
	}

	public updateDateTimeSettings(dateTimeSettings: DateTimeModelDTO): void {
		const userInterfaceSettings = this.getState().settings.userInterfaceSettings;
		this.updateUserInterfaceSettings({ ...userInterfaceSettings, dateTimeSettings });
	}

	public updateDisplaySettings(displaySettings: DisplaySettingsModelDTO): void {
		const userInterfaceSettings = this.getState().settings.userInterfaceSettings;
		this.updateUserInterfaceSettings({ ...userInterfaceSettings, displaySettings });
	}

	public updateMovieViewMode(viewMode: ViewMode): void {
		const displaySettings = this.getState().settings.userInterfaceSettings.displaySettings;
		this.updateDisplaySettings({ ...displaySettings, movieViewMode: viewMode });
	}

	public updateTvShowViewMode(viewMode: ViewMode): void {
		const displaySettings = this.getState().settings.userInterfaceSettings.displaySettings;
		this.updateDisplaySettings({ ...displaySettings, tvShowViewMode: viewMode });
	}

	public updateUserInterfaceSettings(userInterfaceSettings: UserInterfaceSettingsModelDTO): void {
		const settings = this.getState().settings;
		this.setState({
			...this.getState(),
			settings: {
				...settings,
				userInterfaceSettings,
			},
		});
	}
	// endregion

	// region accountSettings

	public getAccountSettings(): Observable<AccountSettingsModelDTO> {
		return this.getSettings().pipe(switchMap((x) => of(x.accountSettings)));
	}

	public getActiveAccountId(): Observable<number> {
		return this.getAccountSettings().pipe(switchMap((x) => of(x?.activeAccountId ?? 0)));
	}

	public updateActiveAccountSettings(activeAccountId: number): void {
		const accountSettings = this.getState().settings.accountSettings;
		this.updateAccountSettings({ ...accountSettings, activeAccountId });
	}

	public updateAccountSettings(accountSettings: AccountSettingsModelDTO): void {
		const settings = this.getState().settings;
		this.setState({
			...this.getState(),
			settings: {
				...settings,
				accountSettings,
			},
		});
	}

	// endregion

	// region advancedSettings

	public getAdvancedSettings(): Observable<AdvancedSettingsModelDTO> {
		return this.getSettings().pipe(switchMap((x: SettingsModelDTO) => of(x.advancedSettings)));
	}

	public getDownloadManagerSettings(): Observable<DownloadManagerModelDTO> {
		return this.getAdvancedSettings().pipe(switchMap((x) => of(x?.downloadManager)));
	}

	public updateAdvancedSettings(advancedSettings: AdvancedSettingsModelDTO): void {
		const settings = this.getState().settings;
		this.setState({
			...this.getState(),
			settings: {
				...settings,
				advancedSettings,
			},
		});
	}

	public updateDownloadManagerSettings(downloadManager: DownloadManagerModelDTO): void {
		const advancedSettings = this.getState().settings.advancedSettings;
		this.updateAdvancedSettings({
			...advancedSettings,
			downloadManager,
		});
	}

	// endregion
}

const settingsService = new SettingsService();
export default settingsService;
