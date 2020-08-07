import { ReplaySubject, Observable, iif } from 'rxjs';
import { tap, mergeMap } from 'rxjs/operators';
import { getActiveAccount, setActiveAccount } from '@api/settingsApi';
import { PlexAccountDTO } from '@dto/mainApi';
import GlobalService from '@service/globalService';
import AccountService from '@service/accountService';
import Log from 'consola';

export class SettingsService {
	private _activeAccount: ReplaySubject<PlexAccountDTO | null> = new ReplaySubject();

	public constructor() {
		GlobalService.getAxiosReady()
			.pipe(
				tap(() => Log.debug('Retrieving accounts')),
				mergeMap(() =>
					AccountService.getAccounts().pipe(
						// Only retrieve the active account if any accounts are available in the database
						mergeMap((accounts) => iif(() => accounts && accounts.length > 0, getActiveAccount())),
					),
				),
			)
			.subscribe((account) => {
				this._activeAccount.next(account);
			});
	}

	public getActiveAccount(): Observable<PlexAccountDTO | null> {
		return this._activeAccount.asObservable();
	}

	public setActiveAccount(accountId: number): void {
		setActiveAccount(accountId)
			.pipe(tap((value) => Log.debug(`SetActiveAccount => ${value?.displayName}`)))
			.subscribe((value) => {
				this._activeAccount.next(value);
			});
	}
}

const settingsService = new SettingsService();
export default settingsService;
