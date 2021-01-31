import Log from 'consola';
import StoreState from '@state/storeState';
import { BaseService } from '@state/baseService';
import { Observable, of } from 'rxjs';
import {
	AccountSettingsModel,
	AdvancedSettingsModel,
	ConfirmationSettingsModel,
	DateTimeModel,
	DownloadManagerModel,
	SettingsModel,
	UserInterfaceSettingsModel,
} from '@dto/mainApi';
import { distinctUntilChanged, finalize, switchMap, take, tap } from 'rxjs/operators';
import GlobalService from '@state/globalService';
import { getSettings, updateSettings } from '@api/settingsApi';
import { isEqual } from 'lodash';

export class SettingsService extends BaseService {
	public constructor() {
		super({
			stateSliceSelector: (state: StoreState) => {
				return {
					settings: state.settings,
				};
			},
		});

		GlobalService.getAxiosReady()
			.pipe(switchMap(() => of(this.fetchSettings())))
			.subscribe();
		this.stateWithPropertyChanges.subscribe((x) => Log.info('stateWith', x));
	}

	// region Settings
	public getSettings(): Observable<SettingsModel | null> {
		return this.stateChanged.pipe(
			switchMap((x) => of(x?.settings ?? null)),
			distinctUntilChanged(isEqual),
		);
	}

	public fetchSettings(): void {
		getSettings()
			.pipe(tap(() => Log.debug('Retrieving settings')))
			.subscribe((settings) => {
				this.setState({ settings });
			});
	}

	public updateSettings(): void {
		const settings = this.getState().settings;
		if (settings) {
			updateSettings(settings)
				.pipe(finalize(() => this.fetchSettings()))
				.subscribe();
		} else {
			Log.warn('SettingsService => updateSettings: settings was invalid, will not send as an update.');
		}
	}
	// endregion

	public getFirstTimeSetup(): Observable<boolean | null> {
		return this.getSettings().pipe(
			switchMap((x) => of(x?.firstTimeSetup ?? null)),
			distinctUntilChanged(isEqual),
		);
	}

	// region UserInterfaceSettings

	public getUserInterfaceSettings(): Observable<UserInterfaceSettingsModel | null> {
		return this.getSettings().pipe(
			switchMap((x) => of(x?.userInterfaceSettings ?? null)),
			distinctUntilChanged(isEqual),
		);
	}

	public getConfirmationSettings(): Observable<ConfirmationSettingsModel | null> {
		return this.getUserInterfaceSettings().pipe(
			switchMap((x) => of(x?.confirmationSettings ?? null)),
			distinctUntilChanged(isEqual),
		);
	}

	public getDateTimeSettings(): Observable<DateTimeModel | null> {
		return this.getUserInterfaceSettings().pipe(
			switchMap((x) => of(x?.dateTimeSettings ?? null)),
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

	public getAccountSettings(): Observable<AccountSettingsModel | null> {
		return this.getSettings().pipe(
			switchMap((x) => of(x?.accountSettings ?? null)),
			distinctUntilChanged(isEqual),
		);
	}

	public getActiveAccountId(): Observable<number> {
		return this.getAccountSettings().pipe(
			switchMap((x) => of(x?.activeAccountId ?? 0)),
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

	public getAdvancedSettings(): Observable<AdvancedSettingsModel | null> {
		return this.getSettings().pipe(
			switchMap((x) => of(x?.advancedSettings ?? null)),
			distinctUntilChanged(isEqual),
		);
	}

	public getDownloadManagerSettings(): Observable<DownloadManagerModel | null> {
		return this.getAdvancedSettings().pipe(
			switchMap((x) => of(x?.downloadManager ?? null)),
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
