import Log from 'consola';
import { Observable, of } from 'rxjs';
import { SettingsModelDTO, ViewMode } from '@dto/mainApi';
import { distinctUntilChanged, filter, map, switchMap, take, tap } from 'rxjs/operators';
import { BaseService, GlobalService } from '@service';
import { getSettings, updateSettings } from '@api/settingsApi';
import { Context } from '@nuxt/types';
import { isEqual } from 'lodash';
import IStoreState from '@interfaces/service/IStoreState';

export class SettingsService extends BaseService {
	// region Constructor and Setup
	public constructor() {
		super({
			// Note: Each service file can only have "unique" state slices which are not also used in other service files
			stateSliceSelector: (state: IStoreState) => {
				return {
					activeAccountId: state.activeAccountId,
					firstTimeSetup: state.firstTimeSetup,
					downloadSegments: state.downloadSegments,
					askDownloadMovieConfirmation: state.askDownloadMovieConfirmation,
					askDownloadTvShowConfirmation: state.askDownloadTvShowConfirmation,
					askDownloadSeasonConfirmation: state.askDownloadSeasonConfirmation,
					askDownloadEpisodeConfirmation: state.askDownloadEpisodeConfirmation,
					tvShowViewMode: state.tvShowViewMode,
					movieViewMode: state.movieViewMode,
					shortDateFormat: state.shortDateFormat,
					longDateFormat: state.longDateFormat,
					timeFormat: state.timeFormat,
					timeZone: state.timeZone,
					showRelativeDates: state.showRelativeDates,
					language: state.language,
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

		this.getFirstTimeSetup().subscribe((state) => {
			if (state === null) {
				return;
			}
			Log.info('Redirecting to the setup page');
			if (state && !nuxtContext.route.path.includes('setup')) {
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
					this.setState(settings, 'Fetch Settings');
				}
			}),
		);
	}
	// endregion

	// region Settings

	public updateSetting<T>(setting: keyof SettingsModelDTO, value: T): void {
		const x = {};
		x[setting] = value;
		this.setState(x, `Update setting: ${setting} with value: ${value}`);
		updateSettings(this.getState()).pipe(take(1)).subscribe();
	}
	// endregion

	public getFirstTimeSetup(): Observable<boolean> {
		return this.stateChanged.pipe(
			map((x) => x?.firstTimeSetup),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	// region ConfirmationSettings
	public getLanguage(): Observable<string> {
		return this.stateChanged.pipe(
			map((x) => x?.language),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}
	// endregion

	// region ConfirmationSettings
	public getAskDownloadMovieConfirmation(): Observable<boolean> {
		return this.stateChanged.pipe(
			map((x) => x?.askDownloadMovieConfirmation),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	public getAskDownloadTvShowConfirmation(): Observable<boolean> {
		return this.stateChanged.pipe(
			map((x) => x?.askDownloadTvShowConfirmation),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	public getAskDownloadSeasonConfirmation(): Observable<boolean> {
		return this.stateChanged.pipe(
			map((x) => x?.askDownloadSeasonConfirmation),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	public getAskDownloadEpisodeConfirmation(): Observable<boolean> {
		return this.stateChanged.pipe(
			map((x) => x?.askDownloadEpisodeConfirmation),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}
	// endregion

	// region UserInterfaceSettings

	// endregion

	// region DateTimeSettings
	public getShortDateFormat(): Observable<string> {
		return this.stateChanged.pipe(
			map((x) => x?.shortDateFormat),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	public getLongDateFormat(): Observable<string> {
		return this.stateChanged.pipe(
			map((x) => x?.longDateFormat),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	public getTimeFormat(): Observable<string> {
		return this.stateChanged.pipe(
			map((x) => x?.timeFormat),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	public getTimeZone(): Observable<string> {
		return this.stateChanged.pipe(
			map((x) => x?.timeZone),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	public getShowRelativeDates(): Observable<boolean> {
		return this.stateChanged.pipe(
			map((x) => x?.showRelativeDates),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}
	// endregion

	// region DisplaySettings
	public getMovieViewMode(): Observable<ViewMode> {
		return this.stateChanged.pipe(
			map((x) => x?.movieViewMode),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}

	public getTvShowViewMode(): Observable<ViewMode> {
		return this.stateChanged.pipe(
			map((x) => x?.tvShowViewMode),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}
	// endregion

	// region accountSettings
	public getActiveAccountId(): Observable<number> {
		return this.stateChanged.pipe(
			map((x) => x?.activeAccountId),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}
	// endregion

	// region advancedSettings
	public getDownloadSegments(): Observable<number> {
		return this.stateChanged.pipe(
			map((x) => x?.downloadSegments),
			filter((x) => !!x),
			distinctUntilChanged(isEqual),
		);
	}
	// endregion
}

const settingsService = new SettingsService();
export default settingsService;
