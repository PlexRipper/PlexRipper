import Log from 'consola';
import { Observable, of, combineLatest } from 'rxjs';
import { getAllAccounts } from '@api/accountApi';
import { PlexAccountDTO } from '@dto/mainApi';
import { switchMap, tap } from 'rxjs/operators';
import SettingsService from '@state/settingsService';
import { BaseService } from '@state/baseService';
import StoreState from '@state/storeState';
import GlobalService from '@state/globalService';

export class AccountService extends BaseService {
	public constructor() {
		super({
			stateSliceSelector: (state: StoreState) => {
				return {
					accounts: state.accounts,
				};
			},
		});

		GlobalService.getAxiosReady()
			.pipe(
				tap(() => Log.debug('Retrieving all accounts')),
				switchMap(() => of(this.fetchAccounts())),
			)
			.subscribe();
	}

	public fetchAccounts(): void {
		getAllAccounts().subscribe((accounts) => {
			Log.debug(`AccountService => Fetch Accounts`, accounts);
			this.setState({ accounts });
		});
	}

	public getAccounts(): Observable<PlexAccountDTO[]> {
		return this.stateChanged.pipe(switchMap((x) => of(x?.accounts ?? [])));
	}

	public getActiveAccount(): Observable<PlexAccountDTO | null> {
		return combineLatest([SettingsService.getActiveAccountId(), this.getAccounts()]).pipe(
			switchMap((result: [number, PlexAccountDTO[]]) => {
				const activeAccountId = result[0];
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
