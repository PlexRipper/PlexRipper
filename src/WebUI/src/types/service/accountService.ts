import { ReplaySubject, Observable } from 'rxjs';
import { getAllAccounts } from '@api/accountApi';
import IPlexAccount from '@dto/IPlexAccount';
import GlobalService from '@service/globalService';

export class AccountService {
	private _accounts: ReplaySubject<IPlexAccount[]> = new ReplaySubject();

	public constructor() {
		GlobalService.getAxiosReady().subscribe(() => {
			this.fetchAccounts();
		});
	}

	public fetchAccounts(): void {
		getAllAccounts().subscribe((value) => this._accounts.next(value ?? []));
	}

	public getAccounts(): Observable<IPlexAccount[]> {
		return this._accounts.asObservable();
	}
}

const accountService = new AccountService();
export default accountService;
