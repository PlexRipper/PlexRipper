import { ReplaySubject, Observable } from 'rxjs';
import { getAllAccounts } from '@api/accountApi';
import { PlexAccountDTO } from '@dto/mainApi';
import GlobalService from '@service/globalService';
import Log from 'consola';

export class AccountService {
	private _accounts: ReplaySubject<PlexAccountDTO[]> = new ReplaySubject();

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

	public getAccounts(): Observable<PlexAccountDTO[]> {
		return this._accounts.asObservable();
	}
}

const accountService = new AccountService();
export default accountService;
