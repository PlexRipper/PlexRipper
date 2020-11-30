import Log from 'consola';
import { ReplaySubject, Observable, of, combineLatest } from 'rxjs';
import { getAllAccounts } from '@api/accountApi';
import { PlexAccountDTO, SettingsModel } from '@dto/mainApi';
import SettingsService from '@service/./settingsService';
import { switchMap, tap } from 'rxjs/operators';

export class AccountService {
	private _accounts: ReplaySubject<PlexAccountDTO[]> = new ReplaySubject();

	public constructor() {
		SettingsService.getSettings()
			.pipe(
				tap(() => Log.debug('Retrieving all accounts')),
				switchMap(() => getAllAccounts()),
			)
			.subscribe((value) => {
				Log.debug(`AccountService => Fetch Accounts`, value);
				this._accounts.next(value ?? []);
			});
	}

	public fetchAccounts(): void {
		getAllAccounts().subscribe((value) => {
			Log.debug(`AccountService => Fetch Accounts`, value);
			this._accounts.next(value ?? []);
		});
	}

	public getAccounts(): Observable<PlexAccountDTO[]> {
		return this._accounts.asObservable();
	}

	public getActiveAccount(): Observable<PlexAccountDTO | null> {
		return combineLatest(SettingsService.getSettings(), this.getAccounts()).pipe(
			switchMap((result: [SettingsModel, PlexAccountDTO[]]) => {
				const activeAccountId = result[0].accountSettings.activeAccountId ?? 0;
				// Check if there is an valid account
				if (activeAccountId > 0) {
					return of(result[1].find((account) => account.id === activeAccountId) ?? null);
				}
				return of(null);
			}),
		);
	}
}

const accountService = new AccountService();
export default accountService;
