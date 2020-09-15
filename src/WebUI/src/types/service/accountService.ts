import { ReplaySubject, Observable, of, combineLatest } from 'rxjs';
import { getAllAccounts } from '@api/accountApi';
import { setActiveAccount } from '@api/settingsApi';
import { PlexAccountDTO } from '@dto/mainApi';
import SettingsService from '@service/./settingsService';
import Log from 'consola';
import { finalize, switchMap, tap } from 'rxjs/operators';

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
			switchMap((result) => {
				const activeAccountId = result[0].accountSettings.activeAccountId;
				// Check if there is an valid account
				if (activeAccountId && activeAccountId > 0) {
					return of(result[1].find((account) => account.id === result[0].accountSettings.activeAccountId) ?? null);
				}
				// Active account id is invalid
				Log.warn('The activeAccountId was invalid of 0, will try to set to the next valid account');
				if (result[1].length > 0) {
					// Set the new active account id
					this.setActiveAccount(result[1][0].id);
					return of(result[1][0]);
				}
				// Active account id cannot be chosen
				Log.warn('No accounts have been defined yet, cannot choose a valid ActiveAccountId');
				return of(null);
			}),
		);
	}

	public setActiveAccount(accountId: number): void {
		setActiveAccount(accountId)
			.pipe(
				tap((value) => Log.debug(`SetActiveAccount => ${value?.displayName}`)),
				finalize(() => SettingsService.fetchSettings()),
			)
			.subscribe();
	}
}

const accountService = new AccountService();
export default accountService;
