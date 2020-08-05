import { ReplaySubject, Observable } from 'rxjs';
import { getAllAccounts } from '@api/accountApi';
import IPlexAccount from '@dto/IPlexAccount';
import GlobalService from '@service/globalService';
import Log from 'consola';

export class AccountService {
	private _accounts: ReplaySubject<IPlexAccount[]> = new ReplaySubject();

	public constructor() {
		GlobalService.getAxiosReady().subscribe(() => {
			this.fetchAccounts();
		});
	}

	public fetchAccounts(): void {
		getAllAccounts().subscribe((value) => {
			Log.debug(`AccountService => Fetch Accounts`, value);
			this._accounts.next(value ?? []);
		});
	}

	public getAccounts(): Observable<IPlexAccount[]> {
		return this._accounts.asObservable();
	}
}

const accountService = new AccountService();
export default accountService;
