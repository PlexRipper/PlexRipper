import Log from 'consola';
import IStoreState from '@interfaces/IStoreState';
import { BaseService } from '@state/baseService';
import { Observable, of } from 'rxjs';
import {
	AccountSettingsModel,
	AdvancedSettingsModel,
	ConfirmationSettingsModel,
	DateTimeModel,
	DisplaySettingsModel,
	DownloadManagerModel,
	SettingsModel,
	UserInterfaceSettingsModel,
	ViewMode,
} from '@dto/mainApi';
import { distinctUntilChanged, filter, switchMap, take, tap } from 'rxjs/operators';
import GlobalService from '@state/globalService';
import { getSettings, updateSettings } from '@api/settingsApi';
import { isEqual } from 'lodash';

export class SettingsService extends BaseService {
	public constructor() {
		super({
			stateSliceSelector: (state: IStoreState) => {
				return {
					settings: state.settings,
				};
			},
		});

		// On app load, request the settings once
		GlobalService.getAxiosReady()
			.pipe(
				tap(() => Log.debug('Retrieving settings')),
				switchMap(() => getSettings()),
				take(1),
			)
			.subscribe((settings) => {
				this.setState({ settings });
			});

		// On settings state update => send update to back-end
		this.getSettings()
			.pipe(switchMap((settings) => updateSettings(settings)))
			.subscribe();
	}

	// region Settings
	public getSettings(): Observable<SettingsModel> {
		return this.stateChanged.pipe(
			filter((x) => x !== null),
			switchMap((x) => of(x.settings)),
			distinctUntilChanged(isEqual),
		);
	}

	public updateSettings(): void {
		const settings = this.getState().settings;
		if (settings) {
			updateSettings(settings).subscribe((settings) => this.setState({ settings }));
		} else {
			Log.warn('SettingsService => updateSettings: settings was invalid, will not send as an update.');
		}
	}
	// endregion

	public getFirstTimeSetup(): Observable<boolean> {
		return this.getSettings().pipe(
			switchMap((x) => of(x.firstTimeSetup)),
			distinctUntilChanged(isEqual),
		);
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

	public getUserInterfaceSettings(): Observable<UserInterfaceSettingsModel> {
		return this.getSettings().pipe(
			switchMap((x) => of(x.userInterfaceSettings)),
			distinctUntilChanged(isEqual),
		);
	}

	public getConfirmationSettings(): Observable<ConfirmationSettingsModel> {
		return this.getUserInterfaceSettings().pipe(
			switchMap((x) => of(x.confirmationSettings)),
			distinctUntilChanged(isEqual),
		);
	}

	public getDisplaySettings(): Observable<DisplaySettingsModel> {
		return this.getUserInterfaceSettings().pipe(
			switchMap((x) => of(x.displaySettings)),
			distinctUntilChanged(isEqual),
		);
	}

	public getDateTimeSettings(): Observable<DateTimeModel> {
		return this.getUserInterfaceSettings().pipe(
			switchMap((x) => of(x.dateTimeSettings)),
			distinctUntilChanged(isEqual),
		);
	}

	public updateConfirmationSettings(confirmationSettings: ConfirmationSettingsModel): void {
		const userInterfaceSettings = this.getState().settings.userInterfaceSettings;
		this.updateUserInterfaceSettings({ ...userInterfaceSettings, confirmationSettings });
	}

	public updateDateTimeSettings(dateTimeSettings: DateTimeModel): void {
		const userInterfaceSettings = this.getState().settings.userInterfaceSettings;
		this.updateUserInterfaceSettings({ ...userInterfaceSettings, dateTimeSettings });
	}

	public updateDisplaySettings(displaySettings: DisplaySettingsModel): void {
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

	public updateUserInterfaceSettings(userInterfaceSettings: UserInterfaceSettingsModel): void {
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

	public getAccountSettings(): Observable<AccountSettingsModel> {
		return this.getSettings().pipe(
			switchMap((x) => of(x.accountSettings)),
			distinctUntilChanged(isEqual),
		);
	}

	public getActiveAccountId(): Observable<number> {
		return this.getAccountSettings().pipe(
			switchMap((x) => of(x.activeAccountId ?? 0)),
			distinctUntilChanged(isEqual),
		);
	}

	public updateActiveAccountSettings(activeAccountId: number): void {
		const accountSettings = this.getState().settings.accountSettings;
		this.updateAccountSettings({ ...accountSettings, activeAccountId });
	}

	public updateAccountSettings(accountSettings: AccountSettingsModel): void {
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

	public getAdvancedSettings(): Observable<AdvancedSettingsModel> {
		return this.getSettings().pipe(
			switchMap((x) => of(x.advancedSettings)),
			distinctUntilChanged(isEqual),
		);
	}

	public getDownloadManagerSettings(): Observable<DownloadManagerModel> {
		return this.getAdvancedSettings().pipe(
			switchMap((x) => of(x.downloadManager)),
			distinctUntilChanged(isEqual),
		);
	}

	public updateAdvancedSettings(advancedSettings: AdvancedSettingsModel): void {
		const settings = this.getState().settings;
		this.setState({
			...this.getState(),
			settings: {
				...settings,
				advancedSettings,
			},
		});
	}

	public updateDownloadManagerSettings(downloadManager: DownloadManagerModel): void {
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
