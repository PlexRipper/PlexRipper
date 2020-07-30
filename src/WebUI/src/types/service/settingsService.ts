import { ReplaySubject, Observable } from 'rxjs';
import { getActiveAccount, setActiveAccount } from '@api/settingsApi';
import IPlexAccount from '@dto/IPlexAccount';
import GlobalService from '@service/globalService';
import AccountService from '@service/accountService';
import Log from 'consola';

export class SettingsService {
	private _activeAccount: ReplaySubject<IPlexAccount | null> = new ReplaySubject();

	public constructor() {
		GlobalService.getAxiosReady().subscribe(() => {
			Log.debug('Retrieving accounts');
			AccountService.getAccounts().subscribe(() => {
				getActiveAccount().subscribe((value) => this._activeAccount.next(value));
			});
		});
	}

	public getActiveAccount(): Observable<IPlexAccount | null> {
		return this._activeAccount.asObservable();
	}

	public setActiveAccount(accountId: number): void {
		setActiveAccount(accountId).subscribe((value) => {
			Log.debug(`SetActiveAccount => ${value?.displayName}`);
			this._activeAccount.next(value);
		});
	}
}

const settingsService = new SettingsService();
export default settingsService;
